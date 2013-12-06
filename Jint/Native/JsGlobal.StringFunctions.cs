// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable StringIndexOfIsCultureSpecific.2
// ReSharper disable StringCompareToIsCultureSpecific
// ReSharper disable StringLastIndexOfIsCultureSpecific.1

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static class StringFunctions
        {
            public static JsBox Constructor(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var target = (JsObject)@this;

                if (target == runtime.Global.GlobalScope)
                {
                    // 15.5.1 - When String is called as a function rather than as a constructor, it performs a type conversion.
                    if (arguments.Length > 0)
                        return JsString.Box(arguments[0].ToString());

                    return JsString.Box(String.Empty);
                }
                else
                {
                    // 15.5.2 - When String is called as part of a new expression, it is a constructor: it initializes the newly created object.
                    target.Value =
                        arguments.Length > 0
                        ? arguments[0].ToString()
                        : String.Empty;

                    return JsBox.CreateObject(target);
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
            public static JsBox ToString(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return ((JsObject)runtime.GetMemberByIndex(@this, Id.valueOf)).Execute(runtime, @this, arguments, null);
            }

            // 15.5.4.3
            public static JsBox ValueOf(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (@this.IsString)
                    return @this;

                return JsString.Box((string)((JsObject)@this).Value);
            }

            // 15.5.4.4
            public static JsBox CharAt(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return ((JsObject)runtime.GetMemberByIndex(@this, Id.substring)).Execute(
                    runtime,
                    @this,
                    new[] { arguments[0], JsNumber.Box(arguments[0].ToNumber() + 1) },
                    null
                );
            }

            // 15.5.4.5
            public static JsBox CharCodeAt(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                var r = @this.ToString();
                var at = (int)arguments[0].ToNumber();

                if (r == String.Empty || at > r.Length - 1)
                    return JsBox.NaN;
                else
                    return JsNumber.Box(Convert.ToInt32(r[at]));
            }

            // 15.5.3.2
            public static JsBox FromCharCode(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                //var r = @this.ToString();

                //if (r == String.Empty || at > r.Length - 1)
                //{
                //    return JsBox.NaN;
                //}
                //else
                //{
                string result = string.Empty;

                foreach (JsBox arg in arguments)
                {
                    result += Convert.ToChar(Convert.ToUInt32(arg.ToNumber()));
                }

                return JsString.Box(result);
                //}
            }

            // 15.5.4.6
            public static JsBox Concat(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(@this.ToString());

                for (int i = 0; i < arguments.Length; i++)
                {
                    sb.Append(arguments[i].ToString());
                }

                return JsString.Box(sb.ToString());
            }

            // 15.5.4.7
            public static JsBox IndexOf(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                string source = @this.ToString();
                string searchString = arguments[0].ToString();
                int position = arguments.Length > 1 ? (int)arguments[1].ToNumber() : 0;

                if (searchString == String.Empty)
                {
                    if (arguments.Length > 1)
                    {
                        return JsNumber.Box(Math.Min(source.Length, position));
                    }
                    else
                    {
                        return JsNumber.Box(0);
                    }
                }

                if (position >= source.Length)
                {
                    return JsNumber.Box(-1);
                }

                return JsNumber.Box(source.IndexOf(searchString, position));
            }

            // 15.5.4.8
            public static JsBox LastIndexOf(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                string source = @this.ToString();
                string searchString = arguments[0].ToString();
                int position = arguments.Length > 1 ? (int)arguments[1].ToNumber() : source.Length;

                return JsNumber.Box(source.Substring(0, position).LastIndexOf(searchString));
            }

            // 15.5.4.9
            public static JsBox LocaleCompare(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(@this.ToString().CompareTo(arguments[0].ToString()));
            }

            // 15.5.4.10
            public static JsBox Match(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                JsObject regexpObject;
                RegExpManager regexp;

                if (TryGetRegExpManager(arguments[0], out regexp))
                {
                    regexpObject = (JsObject)arguments[0];
                }
                else
                {
                    regexpObject = runtime.Global.CreateRegExp(arguments[0].ToString());
                    regexp = (RegExpManager)regexpObject.Value;
                }

                if (!regexp.IsGlobal)
                {
                    return ((JsObject)regexpObject.GetProperty(Id.exec)).Execute(
                        runtime,
                        JsBox.CreateObject(regexpObject),
                        new[] { @this },
                        null
                    );
                }

                var result = runtime.Global.CreateArray();
                var matches = Regex.Matches(@this.ToString(), regexp.Pattern, regexp.Options);

                if (matches.Count > 0)
                {
                    var i = 0;

                    foreach (Match match in matches)
                    {
                        result[JsNumber.Box(i++)] = JsString.Box(match.Value);
                    }

                    return JsBox.CreateObject(result);
                }

                return JsBox.Null;
            }

            // 15.5.4.11
            public static JsBox Replace(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                    return JsString.Box(@this.ToString());

                var searchValue = arguments[0];

                var replaceValue =
                    arguments.Length > 1
                    ? arguments[1]
                    : JsBox.Undefined;

                string source = @this.ToString();
                RegExpManager regexp;

                if (TryGetRegExpManager(searchValue, out regexp))
                    return SearchWithRegExp(runtime, @this, searchValue, regexp, replaceValue, source);

                string search = searchValue.ToString();
                int index = source.IndexOf(search);

                if (index != -1)
                {
                    if (replaceValue.IsFunction)
                    {
                        replaceValue = ((JsObject)replaceValue).Execute(
                            runtime,
                            new JsBox(),
                            new[]
                            {
                                JsString.Box(search),
                                JsNumber.Box(index),
                                JsString.Box(source)
                            },
                            null
                        );

                        return JsString.Box(
                            source.Substring(0, index) +
                            replaceValue +
                            source.Substring(index + search.Length)
                        );
                    }

                    string before = source.Substring(0, index);
                    string after = source.Substring(index + search.Length);
                    string newString = EvaluateReplacePattern(search, before, after, replaceValue.ToString(), null);
                    return JsString.Box(before + newString + after);
                }

                return JsString.Box(source);
            }

            private static JsBox SearchWithRegExp(JintRuntime runtime, JsBox @this, JsBox searchValue, RegExpManager regexp, JsBox replaceValue, string source)
            {
                var regexpObject = (JsObject)searchValue;
                int count = regexp.IsGlobal ? int.MaxValue : 1;
                int lastIndex =
                    regexp.IsGlobal
                        ? 0
                        : Math.Max(
                            0,
                            (int)regexpObject.GetProperty(Id.lastIndex).ToNumber() - 1
                            );

                if (regexp.IsGlobal)
                    regexpObject.SetProperty(Id.lastIndex, JsNumber.Box(0));

                string result;

                if (replaceValue.IsFunction)
                {
                    result = regexp.Regex.Replace(
                        source,
                        m =>
                        {
                            var replaceParameters = new List<JsBox>();
                            if (!regexp.IsGlobal)
                                regexpObject.SetProperty(Id.lastIndex, JsNumber.Box(m.Index + 1));

                            replaceParameters.Add(JsString.Box(m.Value));

                            for (int i = 1; i < m.Groups.Count; i++)
                            {
                                replaceParameters.Add(
                                    m.Groups[i].Success
                                    ? JsString.Box(m.Groups[i].Value)
                                    : JsBox.Undefined
                                );
                            }

                            replaceParameters.Add(JsNumber.Box(m.Index));
                            replaceParameters.Add(JsString.Box(source));

                            return ((JsObject)replaceValue).Execute(
                                runtime,
                                new JsBox(),
                                replaceParameters.ToArray(),
                                null
                                ).ToString();
                        },
                        count,
                        lastIndex
                        );
                }
                else
                {
                    string value = replaceValue.ToString();

                    result = regexp.Regex.Replace(
                        @this.ToString(),
                        m =>
                        {
                            if (!regexp.IsGlobal)
                                regexpObject.SetProperty(Id.lastIndex, JsNumber.Box(m.Index + 1));

                            string after = source.Substring(Math.Min(source.Length - 1, m.Index + m.Length));
                            return EvaluateReplacePattern(
                                m.Value,
                                source.Substring(0, m.Index),
                                after,
                                value,
                                m.Groups
                                );
                        },
                        count,
                        lastIndex
                        );
                }

                return JsString.Box(result);
            }

            // 15.5.4.12
            public static JsBox Search(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                // Converts the arguments to a regex

                RegExpManager regexp;
                if (!TryGetRegExpManager(arguments[0], out regexp))
                {
                    var regexpObject = runtime.Global.CreateRegExp(arguments[0].ToString());
                    regexp = (RegExpManager)regexpObject.Value;
                }

                Match m = regexp.Regex.Match(@this.ToString());

                if (m != null && m.Success)
                    return JsNumber.Box(m.Index);

                return JsNumber.Box(-1);
            }

            // 15.5.4.13
            public static JsBox Slice(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                string source = @this.ToString();
                int start = (int)arguments[0].ToNumber();
                int end = source.Length;
                if (arguments.Length > 1)
                {
                    end = (int)arguments[1].ToNumber();

                    if (end < 0)
                        end = source.Length + end;
                }

                if (start < 0)
                {
                    start = source.Length + start;
                }

                return JsString.Box(source.Substring(start, end - start));
            }

            // 15.5.4.14
            public static JsBox Split(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                JsObject array = runtime.Global.CreateArray();
                string target = @this.ToString();

                if (arguments.Length == 0 || arguments[0].IsUndefined)
                {
                    array.SetProperty(0, JsString.Box(target));
                }

                var separator = arguments[0];
                int limit = arguments.Length > 1 ? Convert.ToInt32(arguments[1].ToNumber()) : Int32.MaxValue;
                string[] result;

                RegExpManager regexp;
                if (TryGetRegExpManager(separator, out regexp))
                    result = regexp.Regex.Split(target, limit);
                else
                    result = target.Split(new[] { separator.ToString() }, limit, StringSplitOptions.None);

                for (int i = 0; i < result.Length; i++)
                {
                    array.SetProperty(i, JsString.Box(result[i]));
                }

                return JsBox.CreateObject(array);
            }

            // 15.5.4.15
            public static JsBox Substring(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                string target = @this.ToString();
                int start = 0, end = target.Length;

                if (arguments.Length > 0 && !double.IsNaN(arguments[0].ToNumber()))
                    start = (int)arguments[0].ToNumber();

                if (
                    arguments.Length > 1 &&
                    !arguments[1].IsUndefined &&
                    !Double.IsNaN(arguments[1].ToNumber())
                )
                    end = (int)arguments[1].ToNumber();

                start = Math.Min(Math.Max(start, 0), Math.Max(0, target.Length - 1));
                end = Math.Min(Math.Max(end, 0), target.Length);
                target = target.Substring(start, end - start);

                return JsString.Box(target);
            }

            public static JsBox Substr(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                string target = @this.ToString();
                int start = 0, end = target.Length;

                if (arguments.Length > 0 && !Double.IsNaN(arguments[0].ToNumber()))
                    start = (int)arguments[0].ToNumber();

                if (
                    arguments.Length > 1 &&
                    !arguments[1].IsUndefined &&
                    !Double.IsNaN(arguments[1].ToNumber())
                )
                    end = (int)arguments[1].ToNumber();

                start = Math.Min(Math.Max(start, 0), Math.Max(0, target.Length - 1));
                end = Math.Min(Math.Max(end, 0), target.Length);
                target = target.Substring(start, end);

                return JsString.Box(target);
            }

            // 15.5.4.16
            public static JsBox ToLowerCase(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsString.Box(@this.ToString().ToLowerInvariant());
            }

            // 15.5.4.17
            public static JsBox ToLocaleLowerCase(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsString.Box(@this.ToString().ToLower());
            }

            // 15.5.4.18
            public static JsBox ToUpperCase(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsString.Box(@this.ToString().ToUpperInvariant());
            }

            // 15.5.4.19
            public static JsBox ToLocaleUpperCase(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsString.Box(@this.ToString().ToUpper());
            }

            // 15.5.5.1
            public static JsBox GetLength(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(@this.ToString().Length);
            }

            private static bool TryGetRegExpManager(JsBox value, out RegExpManager manager)
            {
                if (value.IsObject)
                    manager = ((JsObject)value).Value as RegExpManager;
                else
                    manager = null;

                return manager != null;
            }
        }
    }
}
