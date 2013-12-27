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
            public static object Constructor(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var target = (JsObject)@this;

                if (target == runtime.Global.GlobalScope)
                {
                    // 15.5.1 - When String is called as a function rather than as a constructor, it performs a type conversion.
                    if (arguments.Length > 0)
                        return JsValue.ToString(arguments[0]);

                    return String.Empty;
                }
                else
                {
                    // 15.5.2 - When String is called as part of a new expression, it is a constructor: it initializes the newly created object.
                    target.Value =
                        arguments.Length > 0
                        ? JsValue.ToString(arguments[0])
                        : String.Empty;

                    return target;
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
            public static object ToString(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return ((JsObject)runtime.GetMemberByIndex(@this, Id.valueOf)).Execute(runtime, @this, arguments);
            }

            // 15.5.4.3
            public static object ValueOf(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                if (@this is string)
                    return @this;

                return ((JsObject)@this).Value;
            }

            // 15.5.4.4
            public static object CharAt(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return ((JsObject)runtime.GetMemberByIndex(@this, Id.substring)).Execute(
                    runtime,
                    @this,
                    new[] { arguments[0], JsValue.ToNumber(arguments[0]) + 1 }
                );
            }

            // 15.5.4.5
            public static object CharCodeAt(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var r = JsValue.ToString(@this);
                var at = (int)JsValue.ToNumber(arguments[0]);

                if (r == String.Empty || at > r.Length - 1)
                    return DoubleBoxes.NaN;
                else
                    return (double)r[at];
            }

            // 15.5.3.2
            public static object FromCharCode(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                //var r = @this.ToString();

                //if (r == String.Empty || at > r.Length - 1)
                //{
                //    return object.NaN;
                //}
                //else
                //{
                string result = string.Empty;

                foreach (object arg in arguments)
                {
                    result += (char)(uint)JsValue.ToNumber(arg);
                }

                return result;
                //}
            }

            // 15.5.4.6
            public static object Concat(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(JsValue.ToString(@this));

                for (int i = 0; i < arguments.Length; i++)
                {
                    sb.Append(JsValue.ToString(arguments[i]));
                }

                return sb.ToString();
            }

            // 15.5.4.7
            public static object IndexOf(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                string source = JsValue.ToString(@this);
                string searchString = JsValue.ToString(arguments[0]);
                int position = arguments.Length > 1 ? (int)JsValue.ToNumber(arguments[1]) : 0;

                if (searchString == String.Empty)
                {
                    if (arguments.Length > 1)
                    {
                        return (double)Math.Min(source.Length, position);
                    }
                    else
                    {
                        return (double)0;
                    }
                }

                if (position >= source.Length)
                {
                    return (double)(-1);
                }

                return (double)source.IndexOf(searchString, position);
            }

            // 15.5.4.8
            public static object LastIndexOf(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                string source = JsValue.ToString(@this);
                string searchString = JsValue.ToString(arguments[0]);
                int position = arguments.Length > 1 ? (int)JsValue.ToNumber(arguments[1]) : source.Length;

                return (double)source.Substring(0, position).LastIndexOf(searchString);
            }

            // 15.5.4.9
            public static object LocaleCompare(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return (double)JsValue.ToString(@this).CompareTo(JsValue.ToString(arguments[0]));
            }

            // 15.5.4.10
            public static object Match(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                JsObject regexpObject;
                RegExpManager regexp;

                if (TryGetRegExpManager(arguments[0], out regexp))
                {
                    regexpObject = (JsObject)arguments[0];
                }
                else
                {
                    regexpObject = runtime.Global.CreateRegExp(JsValue.ToString(arguments[0]));
                    regexp = (RegExpManager)regexpObject.Value;
                }

                if (!regexp.IsGlobal)
                {
                    return ((JsObject)regexpObject.GetProperty(Id.exec)).Execute(
                        runtime,
                        regexpObject,
                        new[] { @this }
                    );
                }

                var result = runtime.Global.CreateArray();
                var matches = Regex.Matches(JsValue.ToString(@this), regexp.Pattern, regexp.Options);

                if (matches.Count > 0)
                {
                    var i = 0;

                    foreach (Match match in matches)
                    {
                        result.SetProperty(
                            i++,
                            match.Value
                        );
                    }

                    return result;
                }

                return JsNull.Instance;
            }

            // 15.5.4.11
            public static object Replace(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                if (arguments.Length == 0)
                    return JsValue.ToString(@this);

                var searchValue = arguments[0];

                var replaceValue =
                    arguments.Length > 1
                    ? arguments[1]
                    : JsUndefined.Instance;

                string source = JsValue.ToString(@this);
                RegExpManager regexp;

                if (TryGetRegExpManager(searchValue, out regexp))
                    return SearchWithRegExp(runtime, @this, searchValue, regexp, replaceValue, source);

                string search = JsValue.ToString(searchValue);
                int index = source.IndexOf(search);

                if (index != -1)
                {
                    if (JsValue.IsFunction(replaceValue))
                    {
                        replaceValue = ((JsObject)replaceValue).Execute(
                            runtime,
                            runtime.GlobalScope,
                            new[]
                            {
                                search,
                                (double)index,
                                (object)source
                            }
                        );

                        return source.Substring(0, index) +
                            replaceValue +
                            source.Substring(index + search.Length);
                    }

                    string before = source.Substring(0, index);
                    string after = source.Substring(index + search.Length);
                    string newString = EvaluateReplacePattern(search, before, after, JsValue.ToString(replaceValue), null);
                    return before + newString + after;
                }

                return source;
            }

            private static object SearchWithRegExp(JintRuntime runtime, object @this, object searchValue, RegExpManager regexp, object replaceValue, string source)
            {
                var regexpObject = (JsObject)searchValue;
                int count = regexp.IsGlobal ? int.MaxValue : 1;
                int lastIndex =
                    regexp.IsGlobal
                        ? 0
                        : Math.Max(
                            0,
                            (int)JsValue.ToNumber(regexpObject.GetProperty(Id.lastIndex)) - 1
                            );

                if (regexp.IsGlobal)
                    regexpObject.SetProperty(Id.lastIndex, (double)0);

                string result;

                if (JsValue.IsFunction(replaceValue))
                {
                    if (lastIndex >= source.Length)
                    {
                        result = String.Empty;
                    }
                    else
                    {
                        result = regexp.Regex.Replace(
                            source,
                            m =>
                            {
                                var replaceParameters = new List<object>();
                                if (!regexp.IsGlobal)
                                    regexpObject.SetProperty(Id.lastIndex, (double)(m.Index + 1));

                                replaceParameters.Add(m.Value);

                                for (int i = 1; i < m.Groups.Count; i++)
                                {
                                    replaceParameters.Add(
                                        m.Groups[i].Success
                                        ? (object)m.Groups[i].Value
                                        : JsUndefined.Instance
                                    );
                                }

                                replaceParameters.Add((double)m.Index);
                                replaceParameters.Add(source);

                                return JsValue.ToString(((JsObject)replaceValue).Execute(
                                    runtime,
                                    runtime.GlobalScope,
                                    replaceParameters.ToArray()
                                ));
                            },
                            count,
                            lastIndex
                        );
                    }
                }
                else
                {
                    source = JsValue.ToString(@this);

                    if (lastIndex >= source.Length)
                    {
                        result = String.Empty;
                    }
                    else
                    {
                        string value = JsValue.ToString(replaceValue);

                        result = regexp.Regex.Replace(
                            source,
                            m =>
                            {
                                if (!regexp.IsGlobal)
                                    regexpObject.SetProperty(Id.lastIndex, (double)(m.Index + 1));

                                string after;
                                if (source.Length > 0)
                                    after = source.Substring(Math.Min(source.Length - 1, m.Index + m.Length));
                                else
                                    after = String.Empty;

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
                }

                return result;
            }

            // 15.5.4.12
            public static object Search(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                // Converts the arguments to a regex

                RegExpManager regexp;
                if (!TryGetRegExpManager(arguments[0], out regexp))
                {
                    var regexpObject = runtime.Global.CreateRegExp(JsValue.ToString(arguments[0]));
                    regexp = (RegExpManager)regexpObject.Value;
                }

                Match m = regexp.Regex.Match(JsValue.ToString(@this));

                if (m != null && m.Success)
                    return (double)m.Index;

                return (double)(-1);
            }

            // 15.5.4.13
            public static object Slice(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                string source = JsValue.ToString(@this);
                int start = (int)JsValue.ToNumber(arguments[0]);
                int end = source.Length;
                if (arguments.Length > 1)
                {
                    end = (int)JsValue.ToNumber(arguments[1]);

                    if (end < 0)
                        end = source.Length + end;
                }

                if (start < 0)
                {
                    start = source.Length + start;
                }

                return source.Substring(start, end - start);
            }

            // 15.5.4.14
            public static object Split(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                JsObject array = runtime.Global.CreateArray();
                string target = JsValue.ToString(@this);

                if (arguments.Length == 0 || JsValue.IsUndefined(arguments[0]))
                {
                    array.SetProperty(0, target);
                }

                var separator = arguments[0];
                int limit = arguments.Length > 1 ? (int)JsValue.ToNumber(arguments[1]) : Int32.MaxValue;
                string[] result;

                RegExpManager regexp;
                if (TryGetRegExpManager(separator, out regexp))
                    result = regexp.Regex.Split(target, limit);
                else
                    result = target.Split(new[] { JsValue.ToString(separator) }, limit, StringSplitOptions.None);

                for (int i = 0; i < result.Length; i++)
                {
                    array.SetProperty(i, result[i]);
                }

                return array;
            }

            // 15.5.4.15
            public static object Substring(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                string target = JsValue.ToString(@this);
                int start = 0, end = target.Length;

                if (arguments.Length > 0 && !double.IsNaN(JsValue.ToNumber(arguments[0])))
                    start = (int)JsValue.ToNumber(arguments[0]);

                if (
                    arguments.Length > 1 &&
                    !JsValue.IsUndefined(arguments[1]) &&
                    !Double.IsNaN(JsValue.ToNumber(arguments[1]))
                )
                    end = (int)JsValue.ToNumber(arguments[1]);

                start = Math.Min(Math.Max(start, 0), Math.Max(0, target.Length - 1));
                end = Math.Min(Math.Max(end, 0), target.Length);
                target = target.Substring(start, end - start);

                return target;
            }

            public static object Substr(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                string target = JsValue.ToString(@this);
                int start = 0, end = target.Length;

                if (arguments.Length > 0 && !Double.IsNaN(JsValue.ToNumber(arguments[0])))
                    start = (int)JsValue.ToNumber(arguments[0]);

                if (
                    arguments.Length > 1 &&
                    !JsValue.IsUndefined(arguments[1]) &&
                    !Double.IsNaN(JsValue.ToNumber(arguments[1]))
                )
                    end = (int)JsValue.ToNumber(arguments[1]);

                start = Math.Min(Math.Max(start, 0), Math.Max(0, target.Length - 1));
                end = Math.Min(Math.Max(end, 0), target.Length);
                target = target.Substring(start, end);

                return target;
            }

            // 15.5.4.16
            public static object ToLowerCase(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return JsValue.ToString(@this).ToLowerInvariant();
            }

            // 15.5.4.17
            public static object ToLocaleLowerCase(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return JsValue.ToString(@this).ToLower();
            }

            // 15.5.4.18
            public static object ToUpperCase(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return JsValue.ToString(@this).ToUpperInvariant();
            }

            // 15.5.4.19
            public static object ToLocaleUpperCase(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return JsValue.ToString(@this).ToUpper();
            }

            // 15.5.5.1
            public static object GetLength(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return (double)JsValue.ToString(@this).Length;
            }

            private static bool TryGetRegExpManager(object value, out RegExpManager manager)
            {
                var @object = value as JsObject;
                if (@object != null)
                    manager = @object.Value as RegExpManager;
                else
                    manager = null;

                return manager != null;
            }
        }
    }
}
