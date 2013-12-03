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
            public static JsInstance Constructor(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                JsObject target;

                if (@this == null || @this == runtime.Global.GlobalScope)
                    target = runtime.Global.CreateObject(callee.Prototype);
                else
                    target = (JsObject)@this;

                target.SetClass(callee.Delegate.Name);
                target.SetIsClr(false);

                if (arguments.Length > 0)
                    target.SetProperty(Id.message, arguments[0]);

                return target;
            }

            public static JsInstance ToString(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var target = (JsObject)@this;

                return JsString.Create(target.GetProperty(Id.name) + ": " + target.GetProperty(Id.message));
            }
        }
    }
}
