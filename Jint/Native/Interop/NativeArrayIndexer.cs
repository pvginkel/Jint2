using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native.Interop
{
    internal sealed class NativeArrayPropertyStore<T> : AbstractPropertyStore
    {
        private readonly Marshaller _marshaller;
        private readonly JsGlobal _global;

        public NativeArrayPropertyStore(JsObject owner, Marshaller marshaller)
            : base((DictionaryPropertyStore)owner.PropertyStore)
        {
            _marshaller = marshaller;
            _global = owner.Global;
        }

        public override object GetOwnPropertyRaw(int index)
        {
            if (index >= 0)
            {
                var result = _marshaller.MarshalClrValue(
                    _marshaller.MarshalJsValue<T[]>(JsBox.CreateObject(BaseStore.Owner))[index]
                );

                return result.GetValue();
            }

            return GetOwnPropertyRaw(JsString.Box(_global.GetIdentifier(index)));
        }

        public override object GetOwnPropertyRaw(JsBox index)
        {
            var result = _marshaller.MarshalClrValue(
                _marshaller.MarshalJsValue<T[]>(JsBox.CreateObject(BaseStore.Owner))[_marshaller.MarshalJsValue<int>(index)]
            );

            return result.GetValue();
        }

        public override void DefineProperty(int index, object value, PropertyAttributes attributes)
        {
            if (index >= 0)
            {
                if (attributes != 0)
                    throw new JintException("Cannot set attributes on a native indexable");

                SetPropertyValue(index, JsBox.FromValue(value));
            }
            else
            {
                SetPropertyValue(JsString.Box(_global.GetIdentifier(index)), JsBox.FromValue(value));
            }
        }

        public override void DefineProperty(JsBox index, object value, PropertyAttributes attributes)
        {
            if (attributes != 0)
                throw new JintException("Cannot set attributes on a native indexable");

            SetPropertyValue(index, JsBox.FromValue(value));
        }

        public override void SetPropertyValue(int index, JsBox value)
        {
            if (index >= 0)
                _marshaller.MarshalJsValue<T[]>(JsBox.CreateObject(BaseStore.Owner))[index] = _marshaller.MarshalJsValue<T>(value);
            else
                SetPropertyValue(JsString.Box(_global.GetIdentifier(index)), value);
        }

        public override void SetPropertyValue(JsBox index, JsBox value)
        {
            _marshaller.MarshalJsValue<T[]>(JsBox.CreateObject(BaseStore.Owner))[_marshaller.MarshalJsValue<int>(index)] =
                _marshaller.MarshalJsValue<T>(value);
        }
    }
}
