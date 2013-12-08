using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Jint.Native.Interop
{
    /// <summary>
    /// Represent a set of native overloads to set and get values using indexers.
    /// </summary>
    internal sealed class NativePropertyStore : AbstractPropertyStore
    {
        private readonly JsGlobal _global;
        private readonly NativeOverloadImpl<MethodInfo, WrappedIndexerGetter> _getOverload;
        private readonly NativeOverloadImpl<MethodInfo, WrappedIndexerSetter> _setOverload;

        public NativePropertyStore(JsObject owner, MethodInfo[] getters, MethodInfo[] setters)
            : base((DictionaryPropertyStore)owner.PropertyStore)
        {
            if (getters == null)
                throw new ArgumentNullException("getters");
            if (setters == null)
                throw new ArgumentNullException("setters");

            _global = owner.Global;

            _getOverload = new NativeOverloadImpl<MethodInfo, WrappedIndexerGetter>(
                _global,
                (genericArgs, length) => Array.FindAll(getters, mi => mi.GetParameters().Length == length),
                ProxyHelper.WrapIndexerGetter
            );

            _setOverload = new NativeOverloadImpl<MethodInfo, WrappedIndexerSetter>(
                _global,
                (genericArgs, length) => Array.FindAll(setters, mi => mi.GetParameters().Length == length),
                ProxyHelper.WrapIndexerSetter
            );
        }

        public override object GetOwnPropertyRaw(int index)
        {
            if (index >= 0)
                return GetOwnPropertyRaw(JsString.Box(_global.GetIdentifier(index)));

            return base.GetOwnPropertyRaw(index);
        }

        public override object GetOwnPropertyRaw(JsBox index)
        {
            var getter = _getOverload.ResolveOverload(new[] { index }, null);
            if (getter == null)
                throw new JintException("No matching overload found");

            return getter(_global, BaseStore.Owner, index).GetValue();
        }

        public override void SetPropertyValue(int index, JsBox value)
        {
            if (index >= 0)
                SetPropertyValue(JsString.Box(_global.GetIdentifier(index)), value);
            else
                base.SetPropertyValue(index, value);
        }

        public override void SetPropertyValue(JsBox index, JsBox value)
        {
            var setter = _setOverload.ResolveOverload(new[] { index, value }, null);
            if (setter == null)
                throw new JintException("No matching overload found");

            setter(_global, BaseStore.Owner, index, value);
        }
    }
}
