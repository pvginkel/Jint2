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
            public static object Constructor(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return runtime.Global.Engine.CompileFunction(arguments);
            }

            public static object GetLength(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)((JsObject)@this).Delegate.ArgumentCount;
            }

            public static object SetLength(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return arguments[0];
            }

            public static object ToString(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return String.Format("function {0} ( ) {{ [native code] }}", ((JsObject)@this).Delegate.Name);
            }

            public static object Call(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (!JsValue.IsFunction(@this))
                    throw new ArgumentException("the target of call() must be a function");

                object obj;
                if (arguments.Length >= 1 && !JsValue.IsNullOrUndefined(arguments[0]))
                    obj = arguments[0];
                else
                {
                    obj = runtime.GlobalScope;
                }

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
                return ((JsObject)@this).Execute(runtime, obj, argumentsCopy, null);
            }

            public static object Apply(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (!JsValue.IsFunction(@this))
                    throw new ArgumentException("The target of call() must be a function");

                object obj;

                if (arguments.Length >= 1 && !JsValue.IsNullOrUndefined(arguments[0]))
                    obj = arguments[0];
                else
                {
                    obj = runtime.Global.GlobalScope;
                }

                object[] argumentsCopy;

                if (
                    arguments.Length >= 2 &&
                    arguments[1] is JsObject
                )
                {
                    var argument = (JsObject)arguments[1];

                    int length = (int)JsValue.ToNumber(argument.GetProperty(Id.length));

                    argumentsCopy = new object[length];

                    for (int i = 0; i < length; i++)
                    {
                        argumentsCopy[i] = argument.GetProperty(i);
                    }
                }
                else
                {
                    argumentsCopy = JsValue.EmptyArray;
                }

                // Executes the statements in 'that' and use _this as the target of the call
                return ((JsObject)@this).Execute(runtime, obj, argumentsCopy, null);
            }

            public static object BaseConstructor(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return JsUndefined.Instance;
            }
        }
    }
}
