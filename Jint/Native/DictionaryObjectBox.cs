using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    public struct DictionaryObjectBox
    {
        private JsObject _value;
        private DictionaryPropertyStore _store;

        public JsObject Value
        {
            get { return _value; }
            set
            {
                _value = value;
                _value.EnsurePropertyStore();
                _store = value.PropertyStore as DictionaryPropertyStore;

#if TRACE_SPECULATION
                if (_store == null)
                    Trace.WriteLine("Dictionary store miss");
#endif
            }
        }

        public object GetProperty(int index, ref DictionaryCacheSlot cacheSlot)
        {
            if (_store != null && cacheSlot.Schema == _store.Schema)
            {
#if TRACE_SPECULATION
                // Trace.WriteLine("Dictionary cache hit");
#endif
                return _store.GetOwnPropertyRawUnchecked(cacheSlot.Index);
            }

            return GetPropertySlow(index, ref cacheSlot);
        }

        private object GetPropertySlow(int index, ref DictionaryCacheSlot cacheSlot)
        {
            if (_store != null)
                return _value.GetPropertySlow(index, ref cacheSlot);

#if TRACE_SPECULATION
            if (_store == null)
                Trace.WriteLine("Dictionary cache miss");
#endif
            return _value.GetProperty(index);
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
                _value.SetPropertySlow(index, value, ref cacheSlot);
            }
            else
            {
#if TRACE_SPECULATION
                if (_store == null)
                    Trace.WriteLine("Dictionary cache miss");
#endif
                _value.SetProperty(index, value);
            }
        }
    }
}
