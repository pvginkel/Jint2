using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private JsObject CreatePrototypeSink()
        {
            // This is a special type which is only used as the sink object for
            // prototypes. It's used when we don't have a parent prototype, to
            // make sure that there is something.

            var sink = CreateObject(null);

            sink.SetIsClr(false);
            sink.PropertyStore = SinkPropertyStore.Instance;

            return sink;
        }

        private class SinkPropertyStore : IPropertyStore
        {
            public static readonly SinkPropertyStore Instance = new SinkPropertyStore();

            private static readonly int[] EmptyIntegers = new int[0];

            private SinkPropertyStore()
            {
            }

            public object GetOwnPropertyRaw(int index)
            {
                return null;
            }

            public object GetOwnPropertyRaw(JsBox index)
            {
                return null;
            }

            public void SetPropertyValue(int index, JsBox value)
            {
            }

            public void SetPropertyValue(JsBox index, JsBox value)
            {
            }

            public bool DeleteProperty(int index)
            {
                return true;
            }

            public bool DeleteProperty(JsBox index)
            {
                return true;
            }

            public void DefineProperty(int index, object value, PropertyAttributes attributes)
            {
            }

            public void DefineProperty(JsBox index, object value, PropertyAttributes attributes)
            {
            }

            public IEnumerable<int> GetKeys()
            {
                return EmptyIntegers;
            }
        }
    }
}
