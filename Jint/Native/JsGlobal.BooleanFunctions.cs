using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static class BooleanFunctions
        {
            public static JsBox Constructor(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var target = (JsObject)@this;
                if (target == runtime.Global.GlobalScope)
                    return JsBoolean.Box(arguments.Length > 0 && arguments[0].ToBoolean());

                // e.g., var foo = new Boolean(true);
                if (arguments.Length > 0)
                    target.Value = arguments[0].ToBoolean();
                else
                    target.Value = false;

                return @this;
            }

            public static JsBox ValueOf(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsBox.CreateBoolean(@this.ToBoolean());
            }

            public static JsBox ToString(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsString.Box(JsConvert.ToString(@this.ToBoolean()));
            }
        }
    }
}
