using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    public struct DictionaryUnknownBox
    {
        private object _value;
        private DictionaryPropertyStore _store;

        public object Value
        {
            get { return _value; }
            set
            {
                _value = value;
                var @object = value as JsObject;
                if (@object != null)
                {
                    @object.EnsurePropertyStore();
                    _store = @object.PropertyStore as DictionaryPropertyStore;
                }
                else
                {
                    _store = null;
                }

#if TRACE_SPECULATION
                if (_store == null && !JsValue.IsNullOrUndefined(_value))
                    Trace.WriteLine("Dictionary store miss");
#endif
            }
        }

        public object GetProperty(JintRuntime runtime, int index, ref DictionaryCacheSlot cacheSlot)
        {
            if (_store != null && cacheSlot.Schema == _store.Schema)
            {
#if TRACE_SPECULATION
                // Trace.WriteLine("Dictionary cache hit");
#endif
                return _store.GetOwnPropertyRawUnchecked(cacheSlot.Index);
            }

            return GetPropertySlow(runtime, index, ref cacheSlot);
        }

        private object GetPropertySlow(JintRuntime runtime, int index, ref DictionaryCacheSlot cacheSlot)
        {
            if (_store != null)
            {
                return ((JsObject)_value).GetPropertySlow(index, ref cacheSlot);
            }

#if TRACE_SPECULATION
            Trace.WriteLine("Dictionary cache miss");
#endif
            return runtime.GetMemberByIndex(_value, index);
        }

        public void SetProperty(int index, object value, ref DictionaryCacheSlot cacheSlot)
        {
            if (_store != null && cacheSlot.Schema == _store.Schema)
            {
#if TRACE_SPECULATION
                // Trace.WriteLine("Dictionary cache hit");
#endif
                _store.SetPropertyValueUnchecked(cacheSlot.Index, value);
            }
            else
            {
                SetPropertySlow(index, value, ref cacheSlot);
            }
        }

        private void SetPropertySlow(int index, object value, ref DictionaryCacheSlot cacheSlot)
        {
            if (_store != null)
            {
                if (cacheSlot.Schema == _store.Schema)
                {
#if TRACE_SPECULATION
                    // Trace.WriteLine("Dictionary cache hit");
#endif
                    _store.SetPropertyValueUnchecked(cacheSlot.Index, value);
                }
                else
                {
                    ((JsObject)_value).SetPropertySlow(index, value, ref cacheSlot);
                }
            }
            else
            {
#if TRACE_SPECULATION
                Trace.WriteLine("Dictionary cache miss");
#endif
                JintRuntime.SetMemberByIndex(_value, index, value);
            }
        }
    }
}
