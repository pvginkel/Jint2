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
            public static JsInstance Constructor(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var target = (JsObject)@this;

                if (target == null || target == runtime.Global.GlobalScope)
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
                target.SetProperty(Id.source, JsString.Create(pattern));
                target.SetProperty(Id.lastIndex, JsNumber.Create(0));
                target.SetProperty(Id.global, JsBoolean.Create(options.HasFlag(RegExpOptions.Global)));

                return target;
            }

            public static JsInstance Exec(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var target = (JsObject)@this;
                var regexp = (RegExpManager)target.Value;

                var array = runtime.Global.CreateArray();
                string input = arguments[0].ToString();
                array.SetProperty(Id.input, JsString.Create(input));

                int i = 0;
                var lastIndex = regexp.IsGlobal ? target.GetProperty(Id.lastIndex).ToNumber() : 0;

                var matches = Regex.Matches(input.Substring((int)lastIndex), regexp.Pattern, regexp.Options);
                if (matches.Count == 0)
                    return JsNull.Instance;

                // A[JsNumber.Create(i++)] = JsString.Create(matches[0].Value);
                array.SetProperty(Id.index, JsNumber.Create(matches[0].Index));

                if (regexp.IsGlobal)
                    target.SetProperty(Id.lastIndex, JsNumber.Create(lastIndex + matches[0].Index + matches[0].Value.Length));

                foreach (Group group in matches[0].Groups)
                {
                    array[JsNumber.Create(i++)] = JsString.Create(group.Value);
                }

                return array;
            }

            public static JsInstance Test(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var regexp = (JsObject)@this;
                var matches = ((JsObject)regexp.GetProperty(Id.exec)).Execute(runtime, @this, arguments, null);
                var store = matches.FindArrayStore();

                return JsBoolean.Create(store != null && store.Length > 0);
            }

            public static JsInstance ToString(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var regexp = (RegExpManager)@this.Value;

                return JsString.Create(
                    "/" +
                    regexp.Pattern +
                    "/" +
                    (regexp.IsGlobal ? "g" : String.Empty) +
                    (regexp.IsIgnoreCase ? "i" : String.Empty) +
                    (regexp.IsMultiLine ? "m" : String.Empty)
                );
            }

            public static JsInstance GetLastIndex(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return ((JsObject)@this).GetProperty(Id.lastIndex);
            }
        }
    }
}
