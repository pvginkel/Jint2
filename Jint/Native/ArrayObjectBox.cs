using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    public struct ArrayObjectBox
    {
        private JsObject _value;
        private ArrayPropertyStore _store;

        public JsObject Value
        {
            get { return _value; }
            set
            {
                _value = value;
                _store = value.PropertyStore as ArrayPropertyStore;

#if TRACE_SPECULATION
                if (_store == null)
                    Trace.WriteLine("Array store miss");
#endif
            }
        }

        public object GetProperty(double index)
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
            return _value.GetProperty(index);
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
            _value.SetProperty(index, value);
        }
    }
}
