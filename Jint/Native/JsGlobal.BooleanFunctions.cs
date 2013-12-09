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
            public static object Constructor(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                var target = (JsObject)@this;
                if (target == runtime.Global.GlobalScope)
                    return BooleanBoxes.Box(arguments.Length > 0 && JsValue.ToBoolean(arguments[0]));

                // e.g., var foo = new Boolean(true);
                if (arguments.Length > 0)
                    target.Value = JsValue.ToBoolean(arguments[0]);
                else
                    target.Value = false;

                return @this;
            }

            public static object ValueOf(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return BooleanBoxes.Box(JsValue.ToBoolean(@this));
            }

            public static object ToString(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return JsConvert.ToString(JsValue.ToBoolean(@this));
            }
        }
    }
}
