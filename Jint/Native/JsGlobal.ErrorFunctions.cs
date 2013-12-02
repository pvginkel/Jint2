using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Runtime;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static class ErrorFunctions
        {
            public static JsInstance Constructor(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (@this == null || @this == runtime.Global.GlobalScope)
                {
                    string message = null;
                    if (arguments.Length > 0)
                        message = arguments[0].ToSource();

                    return runtime.Global.CreateError(callee.Prototype, message);
                }

                if (arguments.Length > 0)
                    @this.Value = arguments[0].ToString();
                else
                    @this.Value = String.Empty;

                return @this;
            }

            public static JsInstance ToString(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var target = (JsObject)@this;

                return JsString.Create(target.GetProperty(Id.name) + ": " + target.GetProperty(Id.message));
            }
        }
    }
}
