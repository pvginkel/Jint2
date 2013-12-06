using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native.Interop
{
    internal class NativeArrayPropertyStore<T> : DictionaryPropertyStore
    {
        private readonly Marshaller _marshaller;
        private readonly JsGlobal _global;

        public NativeArrayPropertyStore(JsObject owner, Marshaller marshaller)
            : base(owner)
        {
            _marshaller = marshaller;
            _global = owner.Global;
        }

        public override bool TryGetProperty(JsBox index, out JsBox result)
        {
            result = _marshaller.MarshalClrValue(
                _marshaller.MarshalJsValue<T[]>(JsBox.CreateObject(Owner))[_marshaller.MarshalJsValue<int>(index)]
            );

            return true;
        }

        public override bool TryGetProperty(int index, out JsBox result)
        {
            if (index >= 0)
            {
                result = _marshaller.MarshalClrValue(
                    _marshaller.MarshalJsValue<T[]>(JsBox.CreateObject(Owner))[index]
                );
                return true;
            }

            return TryGetProperty(JsString.Box(_global.GetIdentifier(index)), out result);
        }

        public override bool TrySetProperty(JsBox index, JsBox value)
        {
            _marshaller.MarshalJsValue<T[]>(JsBox.CreateObject(Owner))[_marshaller.MarshalJsValue<int>(index)] =
                _marshaller.MarshalJsValue<T>(value);

            return true;
        }

        public override bool TrySetProperty(int index, JsBox value)
        {
            if (index >= 0)
            {
                _marshaller.MarshalJsValue<T[]>(JsBox.CreateObject(Owner))[index] = _marshaller.MarshalJsValue<T>(value);
                return true;
            }

            return TrySetProperty(JsString.Box(_global.GetIdentifier(index)), value);
        }
    }
}
