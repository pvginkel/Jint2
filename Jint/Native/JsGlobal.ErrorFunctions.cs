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
            public static object Constructor(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var target = (JsObject)@this;
                if (target == runtime.Global.GlobalScope)
                    target = runtime.Global.CreateObject(callee.Prototype);

                target.SetClass(callee.Delegate.Name);
                target.IsClr = false;

                if (arguments.Length > 0)
                    target.SetProperty(Id.message, arguments[0]);

                return target;
            }

            public static object ToString(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var target = (JsObject)@this;

                return target.GetProperty(Id.name) + ": " + target.GetProperty(Id.message);
            }
        }
    }
}
