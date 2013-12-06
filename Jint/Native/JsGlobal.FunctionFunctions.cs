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
            public static JsBox Constructor(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsBox.CreateObject(runtime.Global.Engine.CompileFunction(arguments));
            }

            public static JsBox GetConstructor(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsBox.CreateObject(callee);
            }

            public static JsBox GetLength(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(((JsObject)@this).Delegate.ArgumentCount);
            }

            public static JsBox SetLength(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return arguments[0];
            }

            public static JsBox ToString(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsString.Box(String.Format("function {0} ( ) {{ [native code] }}", callee.Delegate.Name));
            }

            public static JsBox Call(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (!@this.IsFunction)
                    throw new ArgumentException("the target of call() must be a function");

                JsBox obj;
                if (arguments.Length >= 1 && !arguments[0].IsNullOrUndefined)
                    obj = arguments[0];
                else
                    obj = new JsBox();

                JsBox[] argumentsCopy = null;

                if (arguments.Length >= 2 && !arguments[1].IsNull)
                {
                    argumentsCopy = new JsBox[arguments.Length - 1];
                    Array.Copy(arguments, 1, argumentsCopy, 0, argumentsCopy.Length);
                }

                // Executes the statements in 'that' and use _this as the target of the call
                return ((JsObject)@this).Execute(runtime, obj, argumentsCopy, null);
            }

            public static JsBox Apply(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (!@this.IsFunction)
                    throw new ArgumentException("The target of call() must be a function");

                JsBox obj;

                if (arguments.Length >= 1 && !arguments[0].IsNullOrUndefined)
                    obj = arguments[0];
                else
                    obj = new JsBox();

                JsBox[] argumentsCopy = null;

                if (
                    arguments.Length >= 2 &&
                    !arguments[1].IsNull &&
                    arguments[0].IsObject
                )
                {
                    var argument = (JsObject)arguments[0];

                    int length = (int)argument.GetProperty(Id.length).ToNumber();

                    argumentsCopy = new JsBox[length];

                    for (int i = 0; i < length; i++)
                    {
                        argumentsCopy[i] = argument.GetProperty(i);
                    }
                }

                // Executes the statements in 'that' and use _this as the target of the call
                return ((JsObject)@this).Execute(runtime, obj, argumentsCopy, null);
            }

            public static JsBox BaseConstructor(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsBox.Undefined;
            }
        }
    }
}
