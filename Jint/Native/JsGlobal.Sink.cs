﻿using System;
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

            public bool HasOwnProperty(JsInstance index)
            {
                return false;
            }

            public Descriptor GetOwnDescriptor(int index)
            {
                return null;
            }

            public Descriptor GetOwnDescriptor(JsInstance index)
            {
                return null;
            }

            public bool TryGetProperty(JsInstance index, out JsInstance result)
            {
                result = JsUndefined.Instance;
                return true;
            }

            public bool TryGetProperty(int index, out JsInstance result)
            {
                result = JsUndefined.Instance;
                return true;
            }

            public bool TrySetProperty(int index, JsInstance value)
            {
                return true;
            }

            public bool TrySetProperty(JsInstance index, JsInstance value)
            {
                return true;
            }

            public bool Delete(JsInstance index)
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

            public IEnumerator<KeyValuePair<int, JsInstance>> GetEnumerator()
            {
                return JsObject.EmptyKeyValues.GetEnumerator();
            }

            public IEnumerable<JsInstance> GetValues()
            {
                return JsInstance.EmptyArray;
            }

            public IEnumerable<int> GetKeys()
            {
                return EmptyIntegers;
            }
        }
    }
}
