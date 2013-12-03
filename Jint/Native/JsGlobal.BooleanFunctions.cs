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
            public static JsInstance Constructor(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                // e.g., var foo = Boolean(true);
                if (@this == null || @this == runtime.Global.GlobalScope)
                    return JsBoolean.Create(arguments.Length > 0 && arguments[0].ToBoolean());

                // e.g., var foo = new Boolean(true);
                if (arguments.Length > 0)
                    @this.Value = arguments[0].ToBoolean();
                else
                    @this.Value = false;

                return @this;
            }

            public static JsInstance ValueOf(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var jsBoolean = @this as JsBoolean;
                if (jsBoolean != null)
                    return jsBoolean;

                return JsBoolean.Create((bool)@this.Value);
            }

            public static JsInstance ToString(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsString.Create(JsConvert.ToString((bool)@this.Value));
            }
        }
    }
}
