using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Jint.Runtime;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static class StringFunctions
        {
            public static JsInstance Constructor(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (@this == null || @this == runtime.Global.GlobalScope)
                {
                    // 15.5.1 - When String is called as a function rather than as a constructor, it performs a type conversion.
                    if (arguments.Length > 0)
                        return JsString.Create(arguments[0].ToString());

                    return JsString.Create(String.Empty);
                }
                else
                {
                    // 15.5.2 - When String is called as part of a new expression, it is a constructor: it initialises the newly created object.
                    if (arguments.Length > 0)
                        @this.Value = arguments[0].ToString();
                    else
                        @this.Value = String.Empty;

                    return @this;
                }
            }

            private static string EvaluateReplacePattern(string matched, string before, string after, string newString, GroupCollection groups)
            {
                if (newString.Contains("$"))
                {
                    Regex rr = new Regex(@"\$\$|\$&|\$`|\$'|\$\d{1,2}", RegexOptions.Compiled);
                    var res = rr.Replace(newString, delegate(Match m)
                    {
                        switch (m.Value)
                        {
                            case "$$": return "$";
                            case "$&": return matched;
                            case "$`": return before;
                            case "$'": return after;
                            default: int n = int.Parse(m.Value.Substring(1)); return n == 0 ? m.Value : groups[n].Value;
                        }
                    });

                    return res;
                }
                return newString;
            }

            // 15.5.4.2
            public static JsInstance ToString(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return ((JsFunction)runtime.GetMember(@this, "valueOf")).Execute(runtime, @this, arguments, null);
            }

            // 15.5.4.3
            public static JsInstance ValueOf(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var jsString = @this as JsString;
                if (jsString != null)
                    return jsString;

                return JsString.Create((string)@this.Value);
            }

            // 15.5.4.4
            public static JsInstance CharAt(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return ((JsFunction)runtime.GetMember(@this, "substring")).Execute(
                    runtime,
                    @this,
                    new[] { arguments[0], JsNumber.Create(arguments[0].ToNumber() + 1) },
                    null
                );
            }

            // 15.5.4.5
            public static JsInstance CharCodeAt(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var r = @this.ToString();
                var at = (int)arguments[0].ToNumber();

                if (r == String.Empty || at > r.Length - 1)
                {
                    return JsNumber.NaN;
                }
                else
                {
                    return JsNumber.Create(Convert.ToInt32(r[at]));
                }
            }

            // 15.5.3.2
            public static JsInstance FromCharCode(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                //var r = @this.ToString();

                //if (r == String.Empty || at > r.Length - 1)
                //{
                //    return JsNumber.NaN;
                //}
                //else
                //{
                string result = string.Empty;
                foreach (JsInstance arg in arguments)
                    result += Convert.ToChar(Convert.ToUInt32(arg.ToNumber()));

                return JsString.Create(result);
                //}
            }

            // 15.5.4.6
            public static JsInstance Concat(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(@this.ToString());

                for (int i = 0; i < arguments.Length; i++)
                {
                    sb.Append(arguments[i].ToString());
                }

                return JsString.Create(sb.ToString());
            }

            // 15.5.4.7
            public static JsInstance IndexOf(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                string source = @this.ToString();
                string searchString = arguments[0].ToString();
                int position = arguments.Length > 1 ? (int)arguments[1].ToNumber() : 0;

                if (searchString == String.Empty)
                {
                    if (arguments.Length > 1)
                    {
                        return JsNumber.Create(Math.Min(source.Length, position));
                    }
                    else
                    {
                        return JsNumber.Create(0);
                    }
                }

                if (position >= source.Length)
                {
                    return JsNumber.Create(-1);
                }

                return JsNumber.Create(source.IndexOf(searchString, position));
            }

            // 15.5.4.8
            public static JsInstance LastIndexOf(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                string source = @this.ToString();
                string searchString = arguments[0].ToString();
                int position = arguments.Length > 1 ? (int)arguments[1].ToNumber() : source.Length;

                return JsNumber.Create(source.Substring(0, position).LastIndexOf(searchString));
            }

            // 15.5.4.9
            public static JsInstance LocaleCompare(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(@this.ToString().CompareTo(arguments[0].ToString()));
            }

            // 15.5.4.10
            public static JsInstance Match(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                JsRegExp regexp = arguments[0].Class == JsInstance.ClassString
                    ? runtime.Global.CreateRegExp(arguments[0].ToString(), JsRegExpOptions.None)
                    : (JsRegExp)arguments[0];

                if (!regexp.IsGlobal)
                {
                    return ((JsFunction)regexp["exec"]).Execute(runtime, regexp, new[] { @this }, null);
                }
                else
                {
                    var result = runtime.Global.CreateArray();
                    var matches = Regex.Matches(@this.ToString(), regexp.Pattern, regexp.Options);
                    if (matches.Count > 0)
                    {
                        var i = 0;
                        foreach (Match match in matches)
                        {
                            result[JsNumber.Create(i++)] = JsString.Create(match.Value);
                        }

                        return result;
                    }
                    else
                    {
                        return JsNull.Instance;
                    }
                }
            }

            // 15.5.4.11
            public static JsInstance Replace(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                {
                    return JsString.Create(@this.ToString());
                }

                JsInstance searchValue = arguments[0];
                JsInstance replaceValue = JsUndefined.Instance;

                if (arguments.Length > 1)
                {
                    replaceValue = arguments[1];
                }

                string source = @this.ToString();

                JsFunction function = replaceValue as JsFunction;
                if (searchValue.Class == JsInstance.ClassRegexp)
                {
                    int count = ((JsRegExp)arguments[0]).IsGlobal ? int.MaxValue : 1;
                    var regexp = ((JsRegExp)arguments[0]);
                    int lastIndex = regexp.IsGlobal ? 0 : Math.Max(0, (int)regexp["lastIndex"].ToNumber() - 1);

                    if (regexp.IsGlobal)
                    {
                        regexp["lastIndex"] = JsNumber.Create(0);
                    }

                    if (replaceValue is JsFunction)
                    {
                        string ret = ((JsRegExp)arguments[0]).Regex.Replace(source, delegate(Match m)
                        {
                            List<JsInstance> replaceParameters = new List<JsInstance>();
                            if (!regexp.IsGlobal)
                            {
                                regexp["lastIndex"] = JsNumber.Create(m.Index + 1);
                            }

                            replaceParameters.Add(JsString.Create(m.Value));
                            for (int i = 1; i < m.Groups.Count; i++)
                            {
                                if (m.Groups[i].Success)
                                {
                                    replaceParameters.Add(JsString.Create(m.Groups[i].Value));
                                }
                                else
                                {
                                    replaceParameters.Add(JsUndefined.Instance);
                                }
                            }
                            replaceParameters.Add(JsNumber.Create(m.Index));
                            replaceParameters.Add(JsString.Create(source));

                            return runtime.ExecuteFunction(function, null, replaceParameters.ToArray(), null).ToString();
                        }, count, lastIndex);


                        return JsString.Create(ret);

                    }
                    else
                    {
                        string str = arguments[1].ToString();
                        string ret = ((JsRegExp)arguments[0]).Regex.Replace(@this.ToString(), delegate(Match m)
                        {
                            if (!regexp.IsGlobal)
                            {
                                regexp["lastIndex"] = JsNumber.Create(m.Index + 1);
                            }

                            string after = source.Substring(Math.Min(source.Length - 1, m.Index + m.Length));
                            return EvaluateReplacePattern(m.Value, source.Substring(0, m.Index), after, str, m.Groups);
                        }, count, lastIndex);

                        return JsString.Create(ret);
                    }


                }
                else
                {
                    string search = searchValue.ToString();
                    int index = source.IndexOf(search);
                    if (index != -1)
                    {
                        if (replaceValue is JsFunction)
                        {
                            List<JsInstance> replaceParameters = new List<JsInstance>();
                            replaceParameters.Add(JsString.Create(search));
                            replaceParameters.Add(JsNumber.Create(index));
                            replaceParameters.Add(JsString.Create(source));

                            replaceValue = runtime.ExecuteFunction(function, null, replaceParameters.ToArray(), null);

                            return JsString.Create(source.Substring(0, index) + replaceValue.ToString() + source.Substring(index + search.Length));
                        }
                        else
                        {
                            string before = source.Substring(0, index);
                            string after = source.Substring(index + search.Length);
                            string newString = EvaluateReplacePattern(search, before, after, replaceValue.ToString(), null);
                            return JsString.Create(before + newString + after);
                        }
                    }
                    else
                    {
                        return JsString.Create(source);
                    }
                }
            }

            // 15.5.4.12
            public static JsInstance Search(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                // Converts the arguments to a regex
                JsRegExp regExp;

                if (arguments[0].Class == JsInstance.ClassString)
                    regExp = runtime.Global.CreateRegExp(arguments[0].ToString(), JsRegExpOptions.None);
                else
                    regExp = ((JsRegExp)arguments[0]);

                Match m = regExp.Regex.Match(@this.ToString());

                if (m != null && m.Success)
                    return JsNumber.Create(m.Index);
                else
                    return JsNumber.Create(-1);
            }

            // 15.5.4.13
            public static JsInstance Slice(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                string source = @this.ToString();
                int start = (int)arguments[0].ToNumber();
                int end = source.Length;
                if (arguments.Length > 1)
                {
                    end = (int)arguments[1].ToNumber();
                    if (end < 0)
                    {
                        end = source.Length + end;
                    }
                }

                if (start < 0)
                {
                    start = source.Length + start;
                }

                return JsString.Create(source.Substring(start, end - start));
            }

            // 15.5.4.14
            public static JsInstance Split(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                JsObject a = runtime.Global.CreateArray();
                string s = @this.ToString();

                if (arguments.Length == 0 || JsInstance.IsUndefined(arguments[0]))
                {
                    a["0"] = JsString.Create(s);
                }

                JsInstance separator = arguments[0];
                int limit = arguments.Length > 1 ? Convert.ToInt32(arguments[1].ToNumber()) : Int32.MaxValue;
                int length = s.Length;
                string[] result;

                if (separator.Class == JsInstance.ClassRegexp)
                {
                    result = ((JsRegExp)arguments[0]).Regex.Split(s, limit);
                }
                else
                {
                    result = s.Split(new[] { separator.ToString() }, limit, StringSplitOptions.None);
                }

                for (int i = 0; i < result.Length; i++)
                {
                    a[i.ToString()] = JsString.Create(result[i]);
                }

                return a;
            }

            // 15.5.4.15
            public static JsInstance Substring(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                string str = @this.ToString();
                int start = 0, end = str.Length;

                if (arguments.Length > 0 && !double.IsNaN(arguments[0].ToNumber()))
                {
                    start = Convert.ToInt32(arguments[0].ToNumber());
                }

                if (arguments.Length > 1 && !JsInstance.IsUndefined(arguments[1]) && !double.IsNaN(arguments[1].ToNumber()))
                {
                    end = Convert.ToInt32(arguments[1].ToNumber());
                }

                start = Math.Min(Math.Max(start, 0), Math.Max(0, str.Length - 1));
                end = Math.Min(Math.Max(end, 0), str.Length);
                str = str.Substring(start, end - start);

                return JsString.Create(str);
            }

            public static JsInstance Substr(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                string str = @this.ToString();
                int start = 0, end = str.Length;

                if (arguments.Length > 0 && !double.IsNaN(arguments[0].ToNumber()))
                {
                    start = Convert.ToInt32(arguments[0].ToNumber());
                }

                if (arguments.Length > 1 && !JsInstance.IsUndefined(arguments[1]) && !double.IsNaN(arguments[1].ToNumber()))
                {
                    end = Convert.ToInt32(arguments[1].ToNumber());
                }

                start = Math.Min(Math.Max(start, 0), Math.Max(0, str.Length - 1));
                end = Math.Min(Math.Max(end, 0), str.Length);
                str = str.Substring(start, end);

                return JsString.Create(str);
            }

            // 15.5.4.16
            public static JsInstance ToLowerCase(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsString.Create(@this.ToString().ToLowerInvariant());
            }

            // 15.5.4.17
            public static JsInstance ToLocaleLowerCase(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsString.Create(@this.ToString().ToLower());
            }

            // 15.5.4.18
            public static JsInstance ToUpperCase(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsString.Create(@this.ToString().ToUpperInvariant());
            }

            // 15.5.4.19
            public static JsInstance ToLocaleUpperCase(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                string str = @this.ToString();
                return JsString.Create(str.ToUpper());
            }

            // 15.5.5.1
            public static JsInstance GetLength(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                string str = @this.ToString();
                return JsNumber.Create(str.Length);
            }
        }
    }
}
