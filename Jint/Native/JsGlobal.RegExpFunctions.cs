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
            public static object Constructor(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
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
                        foreach (char c in JsValue.ToString(arguments[1]))
                        {
                            switch (c)
                            {
                                case 'm': options |= RegExpOptions.Multiline; break;
                                case 'i': options |= RegExpOptions.IgnoreCase; break;
                                case 'g': options |= RegExpOptions.Global; break;
                            }
                        }
                    }

                    pattern = JsValue.ToString(arguments[0]);
                }

                target.SetClass(JsNames.ClassRegexp);
                target.IsClr = false;
                target.Value = new RegExpManager(pattern, options);
                target.SetProperty(Id.source, pattern);
                target.SetProperty(Id.lastIndex, (double)0);
                target.SetProperty(Id.global, BooleanBoxes.Box(options.HasFlag(RegExpOptions.Global)));

                return target;
            }

            public static object Exec(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                var target = (JsObject)@this;
                var regexp = (RegExpManager)target.Value;

                var array = runtime.Global.CreateArray();
                string input = JsValue.ToString(arguments[0]);
                array.SetProperty(Id.input, input);

                int i = 0;
                var lastIndex = regexp.IsGlobal ? JsValue.ToNumber(target.GetProperty(Id.lastIndex)) : 0;

                var matches = Regex.Matches(input.Substring((int)lastIndex), regexp.Pattern, regexp.Options);
                if (matches.Count == 0)
                    return JsNull.Instance;

                // A[JsNumber.Box(i++)] = JsString.Box(matches[0].Value);
                array.SetProperty(Id.index, (double)matches[0].Index);

                if (regexp.IsGlobal)
                    target.SetProperty(Id.lastIndex, lastIndex + matches[0].Index + matches[0].Value.Length);

                foreach (Group group in matches[0].Groups)
                {
                    array.SetProperty(i++, @group.Value);
                }

                return array;
            }

            public static object Test(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                var regexp = (JsObject)@this;
                var matches = ((JsObject)regexp.GetProperty(Id.exec)).Execute(runtime, @this, arguments, null);
                var store = matches.FindArrayStore();

                return BooleanBoxes.Box(store != null && store.Length > 0);
            }

            public static object ToString(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                var regexp = (RegExpManager)((JsObject)@this).Value;

                return "/" +
                    regexp.Pattern +
                    "/" +
                    (regexp.IsGlobal ? "g" : String.Empty) +
                    (regexp.IsIgnoreCase ? "i" : String.Empty) +
                    (regexp.IsMultiLine ? "m" : String.Empty);
            }

            public static object GetLastIndex(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return ((JsObject)@this).GetProperty(Id.lastIndex);
            }
        }
    }
}
