using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static class RegExpFunctions
        {
            private static DictionaryCacheSlot _execCacheSlot;

            public static object Constructor(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var target = (JsObject)@this;

                if (target == runtime.Global.GlobalScope)
                    target = runtime.Global.CreateObject(callee.Prototype);

                string pattern = null;
                string options = null;

                if (arguments.Length > 0)
                {
                    pattern = JsValue.ToString(arguments[0]);
                    if (arguments.Length > 1)
                        options = JsValue.ToString(arguments[1]);
                }

                var manager = new RegexManager(pattern, options);

                target.SetClass(JsNames.ClassRegexp);
                target.IsClr = false;
                target.Value = manager;
                target.SetProperty(Id.source, pattern);
                target.SetProperty(Id.lastIndex, (double)0);
                target.SetProperty(Id.global, BooleanBoxes.Box(manager.IsGlobal));

                return target;
            }

            public static object Exec(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var target = (JsObject)@this;
                var manager = (RegexManager)target.Value;

                return (object)manager.Exec(runtime, JsValue.ToString(arguments[0])) ?? JsNull.Instance;
            }

            public static object Test(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var matches = ((JsObject)((JsObject)@this).GetProperty(Id.exec, ref _execCacheSlot)).Execute(runtime, @this, arguments);

                return BooleanBoxes.Box(!JsValue.IsNull(matches));
            }

            public static object ToString(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return ((JsObject)@this).Value.ToString();
            }

            public static object GetLastIndex(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return (double)((RegexManager)((JsObject)@this).Value).LastIndex;
            }
        }
    }
}
