using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static class ErrorFunctions
        {
            public static JsBox Constructor(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var target = (JsObject)@this;
                if (target == runtime.Global.GlobalScope)
                    target = runtime.Global.CreateObject(callee.Prototype);

                target.SetClass(callee.Delegate.Name);
                target.SetIsClr(false);

                if (arguments.Length > 0)
                    target.SetProperty(Id.message, arguments[0]);

                return JsBox.CreateObject(target);
            }

            public static JsBox ToString(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var target = (JsObject)@this;

                return JsString.Box(target.GetProperty(Id.name) + ": " + target.GetProperty(Id.message));
            }
        }
    }
}
