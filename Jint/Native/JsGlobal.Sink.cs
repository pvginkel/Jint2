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

            public bool HasOwnProperty(int index)
            {
                return false;
            }

            public bool HasOwnProperty(JsBox index)
            {
                return false;
            }

            public Descriptor GetOwnDescriptor(int index)
            {
                return null;
            }

            public Descriptor GetOwnDescriptor(JsBox index)
            {
                return null;
            }

            public bool TryGetProperty(JsBox index, out JsBox result)
            {
                result = JsBox.Undefined;
                return true;
            }

            public bool TryGetProperty(int index, out JsBox result)
            {
                result = JsBox.Undefined;
                return true;
            }

            public bool TrySetProperty(int index, JsBox value)
            {
                return true;
            }

            public bool TrySetProperty(JsBox index, JsBox value)
            {
                return true;
            }

            public bool Delete(JsBox index)
            {
                return true;
            }

            public bool Delete(int index)
            {
                return true;
            }

            public void DefineOwnProperty(Descriptor currentDescriptor)
            {
            }

            public IEnumerator<KeyValuePair<int, JsBox>> GetEnumerator()
            {
                return JsObject.EmptyKeyValues.GetEnumerator();
            }

            public IEnumerable<JsBox> GetValues()
            {
                return JsBox.EmptyArray;
            }

            public IEnumerable<int> GetKeys()
            {
                return EmptyIntegers;
            }
        }
    }
}
