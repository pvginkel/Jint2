using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Jint.Runtime;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static class FunctionFunctions
        {
            public static JsInstance Constructor(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return runtime.Global.Backend.CompileFunction(arguments);
            }

            public static JsInstance GetConstructor(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return callee;
            }

            public static JsInstance GetLength(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(((JsObject)@this).Length);
            }

            public static JsInstance SetLength(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                int length = (int)arguments[0].ToNumber();

                if (length < 0 || double.IsNaN(length) || double.IsInfinity(length))
                    throw new JsException(JsErrorType.RangeError, "Invalid length");

                var obj = (JsObject)@this;

                obj.Length = length;

                return arguments[0];
            }

            public static JsInstance ToString(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsString.Create(String.Format("function {0} ( ) {{ [native code] }}", callee.Name));
            }

            public static JsInstance Call(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var function = @this as JsFunction;

                if (function == null)
                    throw new ArgumentException("the target of call() must be a function");

                JsInstance obj;

                if (arguments.Length >= 1 && !JsInstance.IsNullOrUndefined(arguments[0]))
                    obj = arguments[0];
                else
                    obj = runtime.Global.GlobalScope;

                JsInstance[] argumentsCopy;

                if (arguments.Length >= 2 && !JsInstance.IsNull(arguments[1]))
                {
                    argumentsCopy = new JsInstance[arguments.Length - 1];
                    Array.Copy(arguments, 1, argumentsCopy, 0, argumentsCopy.Length);
                }
                else
                {
                    argumentsCopy = JsInstance.EmptyArray;
                }

                // Executes the statements in 'that' and use _this as the target of the call
                return function.Execute(runtime, obj, argumentsCopy, null);
            }

            public static JsInstance Apply(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var function = @this as JsFunction;

                if (function == null)
                    throw new ArgumentException("The target of call() must be a function");

                JsInstance obj;

                if (arguments.Length >= 1 && !JsInstance.IsNullOrUndefined(arguments[0]))
                    obj = arguments[0];
                else
                    obj = runtime.Global.GlobalScope;

                JsInstance[] argumentsCopy;

                if (arguments.Length >= 2 && !JsInstance.IsNull(arguments[1]))
                {
                    var argument = arguments[1] as JsObject;
                    if (argument == null)
                        throw new JsException(JsErrorType.TypeError, "Second argument must be an array");

                    argumentsCopy = new JsInstance[argument.Length];

                    for (int i = 0; i < argument.Length; i++)
                    {
                        argumentsCopy[i] = argument[i.ToString(CultureInfo.InvariantCulture)];
                    }
                }
                else
                {
                    argumentsCopy = JsInstance.EmptyArray;
                }

                // Executes the statements in 'that' and use _this as the target of the call
                return function.Execute(runtime, obj, argumentsCopy, null);
            }

            public static JsInstance BaseConstructor(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericarguments)
            {
                return JsUndefined.Instance;
            }
        }
    }
}
