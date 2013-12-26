using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
            // We don't make a distinction between member and indexer
            // getters/setters. Because of this, we have to guess what's trying
            // to be accomplished here. What we do is check whether the base
            // store or prototype store has the property. If not, we return our
            // own index.

            var result = base.GetOwnPropertyRaw(index);
            if (result != null)
                return result;

            if (!BaseStore.Owner.IsPrototypeNull)
            {
                result = BaseStore.Owner.Prototype.GetOwnPropertyRaw(index);
                if (result != null)
                    return result;
            }

            return GetOwnPropertyRaw(_global.GetIdentifier(index));
        }

        public override object GetOwnPropertyRaw(object index)
        {
            var getter = _getOverload.ResolveOverload(new[] { index }, null);
            if (getter == null)
                throw new JintException("No matching overload found");

            return getter(_global, BaseStore.Owner, index);
        }

        public override void SetPropertyValue(int index, object value)
        {
            SetPropertyValue(_global.GetIdentifier(index), value);
        }

        public override void SetPropertyValue(object index, object value)
        {
            var setter = _setOverload.ResolveOverload(new[] { index, value }, null);
            if (setter == null)
                throw new JintException("No matching overload found");

            setter(_global, BaseStore.Owner, index, value);
        }
    }
}
