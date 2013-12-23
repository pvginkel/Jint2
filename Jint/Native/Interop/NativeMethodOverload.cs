using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Jint.Native.Interop
{
    internal class NativeMethodOverload
    {
        private readonly List<MethodInfo> _methods = new List<MethodInfo>();
        private readonly List<MethodInfo> _generics = new List<MethodInfo>();
        private readonly NativeOverloadImpl<MethodInfo, JsFunction> _overloads;

        public NativeMethodOverload(JsGlobal global, IEnumerable<MethodInfo> methods)
        {
            if (global == null)
                throw new ArgumentNullException("global");
            if (methods == null)
                throw new ArgumentNullException("methods");

            foreach (var method in methods)
            {
                if (method.IsGenericMethodDefinition)
                    _generics.Add(method);
                else if (!method.ContainsGenericParameters)
                    _methods.Add(method);
            }

            _overloads = new NativeOverloadImpl<MethodInfo, JsFunction>(
                global,
                GetMembers,
                ProxyHelper.WrapMethod
            );
        }

        public object Execute(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments)
        {
            var genericArguments = JintRuntime.ExtractGenericArguments(ref arguments);

            if (
                _generics.Count == 0 &&
                genericArguments != null &&
                genericArguments.Length > 0
            )
                throw new InvalidOperationException("Method cannot be called without generic arguments");

            var implementation = _overloads.ResolveOverload(
                arguments,
                runtime.Global.Marshaller.MarshalGenericArguments(genericArguments)
            );
            if (implementation == null)
                throw new JintException(String.Format("No matching overload found {0}<{1}>", callee.Delegate.Name, genericArguments));

            return implementation(runtime, @this, callee, closure, arguments);
        }

        private IEnumerable<MethodInfo> GetMembers(Type[] genericArguments, int argumentCount)
        {
            if (genericArguments != null && genericArguments.Length > 0)
            {
                foreach (var item in _generics)
                {
                    // try specialize generics
                    if (item.GetGenericArguments().Length == genericArguments.Length && item.GetParameters().Length == argumentCount)
                    {
                        MethodInfo method = null;
                        try
                        {
                            method = item.MakeGenericMethod(genericArguments);
                        }
                        catch
                        {
                            // Ignore exceptions.
                        }

                        if (method != null)
                            yield return method;
                    }
                }
            }

            foreach (var item in _methods)
            {
                var parameters = item.GetParameters();
                if (parameters.Length != argumentCount)
                    continue;

                yield return item;
            }
        }
    }
}
