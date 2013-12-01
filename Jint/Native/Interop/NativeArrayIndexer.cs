using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native.Interop
{
    internal class NativeArrayPropertyStore<T> : DictionaryPropertyStore
    {
        private readonly Marshaller _marshaller;

        public NativeArrayPropertyStore(JsObject owner, Marshaller marshaller)
            : base(owner)
        {
            _marshaller = marshaller;
        }

        public override bool TryGetProperty(JsInstance index, out JsInstance result)
        {
            result = _marshaller.MarshalClrValue(
                _marshaller.MarshalJsValue<T[]>(Owner)[_marshaller.MarshalJsValue<int>(index)]
            );

            return true;
        }

        public override bool TryGetProperty(string index, out JsInstance result)
        {
            // TODO: Optimize.
            return TryGetProperty(JsString.Create(index), out result);
        }

        public override bool TrySetProperty(JsInstance index, JsInstance value)
        {
            _marshaller.MarshalJsValue<T[]>(Owner)[_marshaller.MarshalJsValue<int>(index)] =
                _marshaller.MarshalJsValue<T>(value);

            return true;
        }

        public override bool TrySetProperty(string index, JsInstance value)
        {
            // TODO: Optimize.
            return TrySetProperty(JsString.Create(index), value);
        }
    }
}
