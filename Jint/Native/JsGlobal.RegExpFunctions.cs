using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static class RegExpFunctions
        {
            public static JsBox Constructor(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var target = (JsObject)@this;

                if (target == runtime.Global.GlobalScope)
                    target = runtime.Global.CreateObject(callee.Prototype);

                string pattern = null;
                var options = RegExpOptions.None;

                if (arguments.Length > 0)
                {
                    if (arguments.Length == 2)
                    {
                        foreach (char c in arguments[1].ToString())
                        {
                            switch (c)
                            {
                                case 'm': options |= RegExpOptions.Multiline; break;
                                case 'i': options |= RegExpOptions.IgnoreCase; break;
                                case 'g': options |= RegExpOptions.Global; break;
                            }
                        }
                    }

                    pattern = arguments[0].ToString();
                }

                target.SetClass(JsNames.ClassRegexp);
                target.SetIsClr(false);
                target.Value = new RegExpManager(pattern, options);
                target.SetProperty(Id.source, JsString.Box(pattern));
                target.SetProperty(Id.lastIndex, JsNumber.Box(0));
                target.SetProperty(Id.global, JsBoolean.Box(options.HasFlag(RegExpOptions.Global)));

                return JsBox.CreateObject(target);
            }

            public static JsBox Exec(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var target = (JsObject)@this;
                var regexp = (RegExpManager)target.Value;

                var array = runtime.Global.CreateArray();
                string input = arguments[0].ToString();
                array.SetProperty(Id.input, JsString.Box(input));

                int i = 0;
                var lastIndex = regexp.IsGlobal ? target.GetProperty(Id.lastIndex).ToNumber() : 0;

                var matches = Regex.Matches(input.Substring((int)lastIndex), regexp.Pattern, regexp.Options);
                if (matches.Count == 0)
                    return JsBox.Null;

                // A[JsNumber.Box(i++)] = JsString.Box(matches[0].Value);
                array.SetProperty(Id.index, JsNumber.Box(matches[0].Index));

                if (regexp.IsGlobal)
                    target.SetProperty(Id.lastIndex, JsNumber.Box(lastIndex + matches[0].Index + matches[0].Value.Length));

                foreach (Group group in matches[0].Groups)
                {
                    array.SetProperty(i++, JsString.Box(group.Value));
                }

                return JsBox.CreateObject(array);
            }

            public static JsBox Test(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var regexp = (JsObject)@this;
                var matches = ((JsObject)regexp.GetProperty(Id.exec)).Execute(runtime, @this, arguments, null);
                var store = matches.FindArrayStore();

                return JsBoolean.Box(store != null && store.Length > 0);
            }

            public static JsBox ToString(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var regexp = (RegExpManager)((JsObject)@this).Value;

                return JsString.Box(
                    "/" +
                    regexp.Pattern +
                    "/" +
                    (regexp.IsGlobal ? "g" : String.Empty) +
                    (regexp.IsIgnoreCase ? "i" : String.Empty) +
                    (regexp.IsMultiLine ? "m" : String.Empty)
                );
            }

            public static JsBox GetLastIndex(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return ((JsObject)@this).GetProperty(Id.lastIndex);
            }
        }
    }
}
