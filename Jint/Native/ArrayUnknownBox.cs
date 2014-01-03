using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    public struct ArrayUnknownBox
    {
        private object _value;
        private ArrayPropertyStore _store;

        public object Value
        {
            get { return _value; }
            set
            {
                _value = value;
                var @object = value as JsObject;
                if (@object != null)
                    _store = @object.PropertyStore as ArrayPropertyStore;
                else
                    _store = null;

#if TRACE_SPECULATION
                if (_store == null)
                    Trace.WriteLine("Array store miss");
#endif
            }
        }

        public object GetProperty(JintRuntime runtime, double index)
        {
            if (_store != null)
            {
                int intIndex = (int)index;
                if (intIndex == index)
                {
#if TRACE_SPECULATION
                    // Trace.WriteLine("Array cache hit");
#endif
                    return _store.GetOwnProperty(intIndex);
                }
            }

#if TRACE_SPECULATION
            Trace.WriteLine("Array cache miss");
#endif
            return runtime.Operation_Member(_value, index);
        }

        public void SetProperty(double index, object value)
        {
            if (_store != null)
            {
                int intIndex = (int)index;
                if (intIndex == index)
                {
#if TRACE_SPECULATION
                    // Trace.WriteLine("Array cache hit");
#endif
                    _store.DefineOrSetPropertyValue(intIndex, value);
                    return;
                }
            }

#if TRACE_SPECULATION
            Trace.WriteLine("Array cache miss");
#endif
            JintRuntime.Operation_SetMember(_value, index, value);
        }
    }
}
