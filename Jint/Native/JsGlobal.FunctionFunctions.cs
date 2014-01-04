using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static class FunctionFunctions
        {
            public static object Constructor(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return runtime.Global.Engine.CompileFunction(arguments);
            }

            public static object GetLength(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return (double)((JsObject)@this).Delegate.ArgumentCount;
            }

            public static object SetLength(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return arguments[0];
            }

            public static object ToString(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return ((JsObject)@this).Delegate.ToString();
            }

            public static object Call(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                if (!JsValue.IsFunction(@this))
                    throw new JsException(JsErrorType.TypeError, "The target of call() must be a function");

                object target;
                if (arguments.Length >= 1 && !JsValue.IsNullOrUndefined(arguments[0]))
                    target = arguments[0];
                else
                    target = runtime.GlobalScope;

                object[] argumentsCopy;

                if (arguments.Length >= 2 && !JsValue.IsNull(arguments[1]))
                {
                    argumentsCopy = new object[arguments.Length - 1];
                    Array.Copy(arguments, 1, argumentsCopy, 0, argumentsCopy.Length);
                }
                else
                {
                    argumentsCopy = JsValue.EmptyArray;
                }

                // Executes the statements in 'that' and use _this as the target of the call
                return ((JsObject)@this).Execute(runtime, target, argumentsCopy);
            }

            public static object Apply(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                if (!JsValue.IsFunction(@this))
                    throw new ArgumentException("The target of call() must be a function");

                object target;

                if (arguments.Length >= 1 && !JsValue.IsNullOrUndefined(arguments[0]))
                    target = arguments[0];
                else
                    target = runtime.Global.GlobalScope;

                object[] argumentsCopy;

                if (arguments.Length >= 2)
                {
                    var shim = new ArrayShim(arguments[1]);

                    argumentsCopy = new object[shim.Length];

                    foreach (var item in shim)
                    {
                        argumentsCopy[item.Key] = item.Value;
                    }
                }
                else
                {
                    argumentsCopy = JsValue.EmptyArray;
                }

                // Executes the statements in 'that' and use _this as the target of the call
                return ((JsObject)@this).Execute(runtime, target, argumentsCopy);
            }

            public static object BaseConstructor(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return JsUndefined.Instance;
            }
        }
    }
}
