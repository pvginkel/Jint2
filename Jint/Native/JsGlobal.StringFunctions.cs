// ReSharper disable StringIndexOfIsCultureSpecific.1, StringIndexOfIsCultureSpecific.2, StringCompareToIsCultureSpecific, StringLastIndexOfIsCultureSpecific.1

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
                    {
                        var argument = arguments[0];
                        if (JsValue.IsString(argument))
                            return argument;
                        return JsValue.ToString(argument);
                    }

                    return String.Empty;
                }
                else
                {
                    // 15.5.2 - When String is called as part of a new expression, it is a constructor: it initializes the newly created object.
                    if (arguments.Length > 0)
                    {
                        var argument = arguments[0];
                        if (JsValue.IsString(argument))
                            target.Value = argument;
                        else
                            target.Value = JsValue.ToString(argument);
                    }
                    else
                    {
                        target.Value = String.Empty;
                    }

                    return target;
                }
            }

            // 15.5.4.2
            public static object ToString(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                if (JsValue.IsString(@this))
                    return @this;

                return ((JsObject)runtime.GetMemberByIndex(@this, Id.valueOf)).Execute(runtime, @this, arguments);
            }

            // 15.5.4.3
            public static object ValueOf(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                if (JsValue.IsString(@this))
                    return @this;

                return ((JsObject)@this).Value;
            }

            // 15.5.4.4
            public static object CharAt(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                if (arguments.Length == 0)
                    return String.Empty;

                int pos = (int)JsValue.ToInteger(arguments[0]);
                if (pos < 0)
                    return String.Empty;

                string value = JsValue.ToString(@this);
                if (pos >= value.Length)
                    return String.Empty;

                return new String(value[pos], 1);
            }

            // 15.5.4.5
            public static object CharCodeAt(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                if (arguments.Length == 0)
                    return String.Empty;

                int pos = (int)JsValue.ToInteger(arguments[0]);
                if (pos < 0)
                    return DoubleBoxes.NaN;

                string value = JsValue.ToString(@this);
                if (pos >= value.Length)
                    return DoubleBoxes.NaN;

                return (double)value[pos];
            }

            // 15.5.3.2
            public static object FromCharCode(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                if (arguments.Length == 0)
                    return String.Empty;
                if (arguments.Length == 1)
                    return new String((char)JsValue.ToUint16(arguments[0]), 1);

                var sb = new StringBuilder(arguments.Length);

                for (int i = 0; i < arguments.Length; i++)
                {
                    sb.Append((char)JsValue.ToUint16(arguments[i]));
                }

                return sb.ToString();
            }

            // 15.5.4.6
            public static object Concat(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var sb = new StringBuilder();

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
                        return (double)Math.Min(source.Length, position);
                    return (double)0;
                }

                if (position >= source.Length)
                    return (double)-1;

                return (double)source.IndexOf(searchString, position);
            }

            // 15.5.4.8
            public static object LastIndexOf(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                string source = JsValue.ToString(@this);
                string searchString = JsValue.ToString(arguments[0]);
                int position = arguments.Length > 1 ? (int)JsValue.ToNumber(arguments[1]) : source.Length;

                return (double)source.LastIndexOf(searchString, position);
            }

            // 15.5.4.9
            public static object LocaleCompare(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return (double)JsValue.ToString(@this).CompareTo(JsValue.ToString(arguments[0]));
            }

            // 15.5.4.10
            public static object Match(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                RegexManager manager;

                if (!TryGetRegExpManager(arguments[0], out manager))
                    manager = new RegexManager(JsValue.ToString(arguments[0]), null);

                var input = JsValue.ToString(@this);

                if (!manager.IsGlobal)
                    return manager.Exec(runtime, input);

                var matches = manager.Regex.Matches(input);
                if (matches.Count == 0)
                    return JsNull.Instance;

                var result = runtime.Global.CreateArray();
                var store = result.FindArrayStore();

                for (int i = 0; i < matches.Count; i++)
                {
                    store.DefineOrSetPropertyValue(i, matches[i].Value);
                }

                return result;
            }

            // 15.5.4.11
            public static object Replace(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                if (arguments.Length == 0)
                    return JsValue.ToString(@this);

                var searchValue = arguments[0];
                var replaceValue = arguments.Length > 1 ? arguments[1] : JsUndefined.Instance;

                string source = JsValue.ToString(@this);

                RegexManager manager;
                if (TryGetRegExpManager(searchValue, out manager))
                    return ReplaceWithManager(runtime, manager, replaceValue, source);

                string search = JsValue.ToString(searchValue);
                int index = source.IndexOf(search);

                if (index == -1)
                    return source;

                if (JsValue.IsFunction(replaceValue))
                {
                    replaceValue = ((JsObject)replaceValue).Execute(
                        runtime,
                        runtime.GlobalScope,
                        search,
                        index,
                        source
                    );

                    return
                        source.Substring(0, index) +
                        JsValue.ToString(replaceValue) +
                        source.Substring(index + search.Length);
                }

                var sb = new StringBuilder(source, 0, index, source.Length);

                EvaluateReplacePattern(sb, source, search, index, JsValue.ToString(replaceValue), null);

                int count = source.Length - (index + search.Length);
                if (count > 0)
                    sb.Append(source, index + search.Length, count);

                return sb.ToString();
            }

            private static object ReplaceWithManager(JintRuntime runtime, RegexManager manager, object replaceValue, string source)
            {
                int count;
                int lastIndex;

                if (manager.IsGlobal)
                {
                    count = int.MaxValue;
                    lastIndex = 0;
                    manager.LastIndex = 0;
                }
                else
                {
                    count = 1;
                    lastIndex = Math.Max(0, manager.LastIndex - 1);
                }

                if (lastIndex >= source.Length)
                    return String.Empty;

                JsDelegate function = null;
                string replace = null;
                bool replacePattern = false;

                if (JsValue.IsFunction(replaceValue))
                {
                    function = ((JsObject)replaceValue).Delegate;
                }
                else
                {
                    replace = JsValue.ToString(replaceValue);
                    replacePattern = replace.IndexOf('$') != -1;
                }

                var sb = new StringBuilder();
                int offset = 0;

                foreach (Match match in manager.Regex.Matches(source, lastIndex))
                {
                    if (count-- == 0)
                        break;

                    if (match.Index > offset)
                        sb.Append(source, offset, match.Index - offset);

                    offset = match.Index + match.Length;

                    if (!manager.IsGlobal)
                        manager.LastIndex = match.Index + 1;

                    if (replace != null)
                    {
                        if (replacePattern)
                        {
                            EvaluateReplacePattern(
                                sb,
                                source,
                                match.Value,
                                match.Index,
                                replace,
                                match.Groups
                            );
                        }
                        else
                        {
                            sb.Append(replace);
                        }
                    }
                    else
                    {
                        var replaceArguments = new object[match.Groups.Count + 2];

                        for (int i = 0; i < match.Groups.Count; i++)
                        {
                            replaceArguments[i] = match.Groups[i].Success
                                ? (object)match.Groups[i].Value
                                : JsUndefined.Instance;
                        }

                        replaceArguments[replaceArguments.Length - 2] = match.Index;
                        replaceArguments[replaceArguments.Length - 1] = source;

                        sb.Append(JsValue.ToString(function.Delegate(
                            runtime,
                            runtime.GlobalScope,
                            (JsObject)replaceValue,
                            replaceArguments
                        )));
                    }
                }

                sb.Append(source, offset, source.Length - offset);

                return sb.ToString();
            }

            private static void EvaluateReplacePattern(StringBuilder sb, string source, string matched, int matchOffset, string replacement, GroupCollection groups)
            {
                int offset = 0;
                int count;

                for (int i = 0; i < replacement.Length; i++)
                {
                    if (replacement[i] != '$')
                        continue;

                    if (i > offset)
                    {
                        sb.Append(replacement, offset, i - offset);
                        offset = i;
                    }

                    switch (replacement[++i])
                    {
                        case '$':
                            // Move the offset to the current index, so the
                            // next append will include the $.
                            offset = i;
                            break;

                        case '&':
                            sb.Append(matched);
                            offset = i + 1;
                            break;

                        case '`':
                            if (matchOffset > 0)
                                sb.Append(source, 0, matchOffset);
                            offset = i + 1;
                            break;

                        case '\'':
                            count = source.Length - (matchOffset + matched.Length);
                            if (count > 0)
                                sb.Append(source, matchOffset + matched.Length, count);
                            break;

                        default:
                            int digit = GetDigit(replacement[i]);
                            int index = 0;
                            if (digit != -1)
                            {
                                index = digit;
                                if (i + 1 < replacement.Length)
                                {
                                    digit = GetDigit(replacement[i + 1]);
                                    if (digit != -1)
                                    {
                                        i++;
                                        index = index * 10 + digit;
                                    }
                                }
                            }

                            // If we have index 0, we leave the offset at
                            // the current position so the token will still
                            // be included.
                            if (index > 0)
                            {
                                offset = i + 1;
                                if (groups != null && index < groups.Count)
                                    sb.Append(groups[i].Value);
                            }

                            break;
                    }
                }

                count = replacement.Length - offset;
                if (count > 0)
                    sb.Append(replacement, offset, count);
            }

            private static int GetDigit(char c)
            {
                int result = c - '0';
                if (result >= 0 && result <= 9)
                    return result;
                return -1;
            }

            // 15.5.4.12
            public static object Search(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                // Converts the arguments to a regex

                RegexManager manager;
                if (!TryGetRegExpManager(arguments[0], out manager))
                    manager = new RegexManager(JsValue.ToString(arguments[0]), null);

                var input = JsValue.ToString(@this);

                var match = manager.Regex.Match(input);
                if (match.Success)
                    return (double)match.Index;

                return (double)-1;
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
                    start = source.Length + start;

                return source.Substring(start, end - start);
            }

            // 15.5.4.14
            public static object Split(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var array = runtime.Global.CreateArray();
                var store = array.FindArrayStore();
                string target = JsValue.ToString(@this);

                if (arguments.Length == 0 || JsValue.IsUndefined(arguments[0]))
                {
                    store.DefineOrSetPropertyValue(0, target);
                    return array;
                }

                var separator = arguments[0];
                int limit = arguments.Length > 1 ? (int)JsValue.ToNumber(arguments[1]) : int.MaxValue;
                string[] result;

                RegexManager manager;
                if (TryGetRegExpManager(separator, out manager))
                    result = manager.Regex.Split(target, limit);
                else
                    result = target.Split(new[] { JsValue.ToString(separator) }, limit, StringSplitOptions.None);

                for (int i = 0; i < result.Length; i++)
                {
                    store.DefineOrSetPropertyValue(i, result[i]);
                }

                return array;
            }

            // 15.5.4.15
            public static object Substring(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                string target = JsValue.ToString(@this);
                int start = 0;
                int end = target.Length;

                if (arguments.Length > 0)
                {
                    double number = JsValue.ToNumber(arguments[0]);
                    if (!Double.IsNaN(number))
                        start = (int)JsValue.ToNumber(arguments[0]);
                }

                if (arguments.Length > 1 && !JsValue.IsUndefined(arguments[1]))
                {
                    double number = JsValue.ToNumber(arguments[1]);
                    if (!Double.IsNaN(number))
                        end = (int)JsValue.ToNumber(arguments[1]);
                }

                start = Math.Min(Math.Max(start, 0), Math.Max(0, target.Length - 1));
                end = Math.Min(Math.Max(end, 0), target.Length);

                return target.Substring(start, end - start);
            }

            public static object Substr(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                string target = JsValue.ToString(@this);
                int start = 0, end = target.Length;

                if (arguments.Length > 0)
                {
                    var number = JsValue.ToNumber(arguments[0]);
                    if (!Double.IsNaN(number))
                        start = (int)number;
                }

                if (arguments.Length > 1 && !JsValue.IsUndefined(arguments[1]))
                {
                    double number = JsValue.ToNumber(arguments[1]);
                    if (!Double.IsNaN(number))
                        end = (int)number;
                }

                start = Math.Min(Math.Max(start, 0), Math.Max(0, target.Length - 1));
                end = Math.Min(Math.Max(end, 0), target.Length);

                return target.Substring(start, end);
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

            private static bool TryGetRegExpManager(object value, out RegexManager manager)
            {
                var @object = value as JsObject;
                if (@object != null)
                    manager = @object.Value as RegexManager;
                else
                    manager = null;

                return manager != null;
            }
        }
    }
}
