using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Jint.Native.Interop
{
    /// <summary>
    /// Represent a set of native overloads to set and get values using indexers.
    /// </summary>
    public class NativeIndexer : INativeIndexer
    {
        private readonly JsGlobal _global;
        private readonly NativeOverloadImpl<MethodInfo, WrappedIndexerGetter> _getOverload;
        private readonly NativeOverloadImpl<MethodInfo, WrappedIndexerSetter> _setOverload;

        public NativeIndexer(JsGlobal global, MethodInfo[] getters, MethodInfo[] setters)
        {
            if (global == null)
                throw new ArgumentNullException("global");
            if (getters == null)
                throw new ArgumentNullException("getters");
            if (setters == null)
                throw new ArgumentNullException("setters");

            _global = global;

            _getOverload = new NativeOverloadImpl<MethodInfo, WrappedIndexerGetter>(
                global,
                (genericArgs, length) => Array.FindAll(getters, mi => mi.GetParameters().Length == length),
                ProxyHelper.WrapIndexerGetter
            );

            _setOverload = new NativeOverloadImpl<MethodInfo, WrappedIndexerSetter>(
                global,
                (genericArgs, length) => Array.FindAll(setters, mi => mi.GetParameters().Length == length),
                ProxyHelper.WrapIndexerSetter
            );
        }

        public JsInstance Get(JsInstance that, JsInstance index)
        {
            var getter = _getOverload.ResolveOverload(new[] { index }, null);
            if (getter == null)
                throw new JintException("No matching overload found");

            return getter(_global, that, index);
        }

        public void Set(JsInstance that, JsInstance index, JsInstance value)
        {
            var setter = _setOverload.ResolveOverload(new[] { index, value }, null);
            if (setter == null)
                throw new JintException("No matching overload found");

            setter(_global, that, index, value);
        }
    }
}
