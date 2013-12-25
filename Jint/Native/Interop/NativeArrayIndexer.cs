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
                    _marshaller.MarshalJsValue<T[]>(BaseStore.Owner)[index]
                );

                return result;
            }

            return GetOwnPropertyRaw(_global.GetIdentifier(index));
        }

        public override object GetOwnPropertyRaw(object index)
        {
            return _marshaller.MarshalClrValue(
                _marshaller.MarshalJsValue<T[]>(BaseStore.Owner)[_marshaller.MarshalJsValue<int>(index)]
            );
        }

        public override void DefineProperty(int index, object value, PropertyAttributes attributes)
        {
            if (index >= 0)
            {
                if (attributes != 0)
                    throw new JintException("Cannot set attributes on a native indexable");

                SetPropertyValue(index, value);
            }
            else
            {
                SetPropertyValue(_global.GetIdentifier(index), value);
            }
        }

        public override void DefineProperty(object index, object value, PropertyAttributes attributes)
        {
            if (attributes != 0)
                throw new JintException("Cannot set attributes on a native indexable");

            SetPropertyValue(index, value);
        }

        public override void SetPropertyValue(int index, object value)
        {
            if (index >= 0)
                _marshaller.MarshalJsValue<T[]>(BaseStore.Owner)[index] = _marshaller.MarshalJsValue<T>(value);
            else
                SetPropertyValue(_global.GetIdentifier(index), value);
        }

        public override void SetPropertyValue(object index, object value)
        {
            _marshaller.MarshalJsValue<T[]>(BaseStore.Owner)[_marshaller.MarshalJsValue<int>(index)] =
                _marshaller.MarshalJsValue<T>(value);
        }
    }
}
