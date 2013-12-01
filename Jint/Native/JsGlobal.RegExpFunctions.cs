﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Jint.Runtime;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static class RegExpFunctions
        {
            public static JsInstance Constructor(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                string pattern = null;
                var options = JsRegExpOptions.None;

                if (arguments.Length > 0)
                {
                    if (arguments.Length == 2)
                    {
                        foreach (char c in arguments[1].ToString())
                        {
                            switch (c)
                            {
                                case 'm': options |= JsRegExpOptions.Multiline; break;
                                case 'i': options |= JsRegExpOptions.IgnoreCase; break;
                                case 'g': options |= JsRegExpOptions.Global; break;
                            }
                        }
                    }

                    pattern = arguments[0].ToString();
                }

                return runtime.Global.CreateRegExp(pattern, options);
            }

            public static JsInstance Exec(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var regexp = (JsRegExp)@this;
                JsArray a = runtime.Global.CreateArray();
                string input = arguments[0].ToString();
                a["input"] = JsString.Create(input);

                int i = 0;
                var lastIndex = regexp.IsGlobal ? regexp["lastIndex"].ToNumber() : 0;
                MatchCollection matches = Regex.Matches(input.Substring((int)lastIndex), regexp.Pattern, regexp.Options);
                if (matches.Count > 0)
                {
                    // A[JsNumber.Create(i++)] = JsString.Create(matches[0].Value);
                    a["index"] = JsNumber.Create(matches[0].Index);

                    if (regexp.IsGlobal)
                    {
                        regexp["lastIndex"] = JsNumber.Create(lastIndex + matches[0].Index + matches[0].Value.Length);
                    }

                    foreach (Group g in matches[0].Groups)
                    {
                        a[JsNumber.Create(i++)] = JsString.Create(g.Value);
                    }

                    return a;
                }
                else
                {
                    return JsNull.Instance;
                }

            }

            public static JsInstance Test(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var regexp = (JsRegExp)@this;
                var array = ((JsFunction)regexp["exec"]).Execute(runtime, @this, arguments, null) as JsArray;
                return JsBoolean.Create(array != null && array.Length > 0);
            }

            public static JsInstance ToString(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var regexp = (JsRegExp)@this;

                return JsString.Create(
                    "/" +
                    regexp.Pattern +
                    "/" +
                    (regexp.IsGlobal ? "g" : String.Empty) +
                    (regexp.IsIgnoreCase ? "i" : String.Empty) +
                    (regexp.IsMultiLine ? "m" : String.Empty)
                );
            }

            public static JsInstance GetLastIndex(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return ((JsRegExp)@this)["lastIndex"];
            }
        }
    }
}