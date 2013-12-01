using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Jint.Native.Interop
{
    /// <summary>
    /// Represent a set of native overloads to set and get values using indexers.
    /// </summary>
    internal class NativePropertyStore : DictionaryPropertyStore
    {
        private readonly JsGlobal _global;
        private readonly NativeOverloadImpl<MethodInfo, WrappedIndexerGetter> _getOverload;
        private readonly NativeOverloadImpl<MethodInfo, WrappedIndexerSetter> _setOverload;

        public NativePropertyStore(JsObject owner, MethodInfo[] getters, MethodInfo[] setters)
            : base(owner)
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

        public override bool TryGetProperty(int index, out JsInstance result)
        {
            // TODO: Optimize.
            return TryGetProperty(JsString.Create(_global.GetIdentifier(index)), out result);
        }

        public override bool TryGetProperty(JsInstance index, out JsInstance result)
        {
            var getter = _getOverload.ResolveOverload(new[] { index }, null);
            if (getter == null)
                throw new JintException("No matching overload found");

            result = getter(_global, Owner, index);
            return true;
        }

        public override bool TrySetProperty(int index, JsInstance value)
        {
            // TODO: Optimize.
            return TrySetProperty(JsString.Create(_global.GetIdentifier(index)), value);
        }

        public override bool TrySetProperty(JsInstance index, JsInstance value)
        {
            var setter = _setOverload.ResolveOverload(new[] { index, value }, null);
            if (setter == null)
                throw new JintException("No matching overload found");

            setter(_global, Owner, index, value);
            return true;
        }
    }
}
