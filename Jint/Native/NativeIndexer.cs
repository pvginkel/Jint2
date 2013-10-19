using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Jint.Marshal;

namespace Jint.Native
{
    /// <summary>
    /// Represent a set of native overloads to set and get values using indexers.
    /// </summary>
    public class NativeIndexer : INativeIndexer
    {
        private readonly NativeOverloadImpl<MethodInfo, JsIndexerGetter> _getOverload;
        private readonly NativeOverloadImpl<MethodInfo, JsIndexerSetter> _setOverload;

        public NativeIndexer(Marshaller marshaller, MethodInfo[] getters, MethodInfo[] setters)
        {
            _getOverload = new NativeOverloadImpl<MethodInfo, JsIndexerGetter>(
                marshaller,
                delegate(Type[] genericArgs, int length)
                {
                    return Array.FindAll<MethodInfo>(getters, mi => mi.GetParameters().Length == length);
                },
                new NativeOverloadImpl<MethodInfo, JsIndexerGetter>.WrapMemberDelegate(marshaller.WrapIndexerGetter)
            );
            _setOverload = new NativeOverloadImpl<MethodInfo, JsIndexerSetter>(
                marshaller,
                delegate(Type[] genericArgs, int length)
                {
                    return Array.FindAll<MethodInfo>(setters, mi => mi.GetParameters().Length == length);
                },
                new NativeOverloadImpl<MethodInfo,JsIndexerSetter>.WrapMemberDelegate(marshaller.WrapIndexerSetter)
            );
        }

        public JsInstance Get(JsInstance that, JsInstance index)
        {
            JsIndexerGetter getter = _getOverload.ResolveOverload(new JsInstance[] { index }, null);
            if (getter == null)
                throw new JintException("No matching overload found");
            return getter(that, index);
        }

        public void Set(JsInstance that, JsInstance index, JsInstance value)
        {
            JsIndexerSetter setter = _setOverload.ResolveOverload(new JsInstance[] { index, value }, null);
            if (setter == null)
                throw new JintException("No matching overload found");

            setter(that, index, value);
        }
    }
}
