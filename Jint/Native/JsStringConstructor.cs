using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Jint.Native
{
    [Serializable]
    public class JsStringConstructor : JsConstructor
    {
        public JsStringConstructor(JsGlobal global)
            : base(global, BuildPrototype(global))
        {
            Name = "String";

            this["fromCharCode"] = global.FunctionClass.New<JsInstance>(FromCharCodeImpl);
        }

        private static JsObject BuildPrototype(JsGlobal global)
        {
            var prototype = new JsObject(global, global.FunctionClass.Prototype);

            prototype.DefineOwnProperty("split", global.FunctionClass.New<JsInstance>(SplitImpl, 2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("replace", global.FunctionClass.New<JsInstance>(ReplaceImpl, 2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toString", global.FunctionClass.New<JsInstance>(ToStringImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toLocaleString", global.FunctionClass.New<JsInstance>(ToStringImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("match", global.FunctionClass.New<JsInstance>(MatchFunc), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("localeCompare", global.FunctionClass.New<JsInstance>(LocaleCompareImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("substring", global.FunctionClass.New<JsInstance>(SubstringImpl, 2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("substr", global.FunctionClass.New<JsInstance>(SubstrImpl, 2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("search", global.FunctionClass.New<JsInstance>(SearchImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("valueOf", global.FunctionClass.New<JsInstance>(ValueOfImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("concat", global.FunctionClass.New<JsInstance>(ConcatImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("charAt", global.FunctionClass.New<JsInstance>(CharAtImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("charCodeAt", global.FunctionClass.New<JsInstance>(CharCodeAtImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("lastIndexOf", global.FunctionClass.New<JsInstance>(LastIndexOfImpl, 1), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("indexOf", global.FunctionClass.New<JsInstance>(IndexOfImpl, 1), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toLowerCase", global.FunctionClass.New<JsInstance>(ToLowerCaseImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toLocaleLowerCase", global.FunctionClass.New<JsInstance>(ToLocaleLowerCaseImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toUpperCase", global.FunctionClass.New<JsInstance>(ToUpperCaseImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toLocaleUpperCase", global.FunctionClass.New<JsInstance>(ToLocaleUpperCaseImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("slice", global.FunctionClass.New<JsInstance>(SliceImpl, 2), PropertyAttributes.DontEnum);

            #region Properties
            prototype.DefineOwnProperty(new PropertyDescriptor<JsInstance>(global, prototype, "length", LengthImpl));
            #endregion

            return prototype;
        }

        public override JsFunctionResult Execute(JsInstance that, JsInstance[] parameters, Type[] genericArguments)
        {
            if (that == null || that == Global.GlobalScope)
            {
                JsInstance result;

                // 15.5.1 - When String is called as a function rather than as a constructor, it performs a type conversion.
                if (parameters.Length > 0)
                    result = JsString.Create(parameters[0].ToString());
                else
                    result = JsString.Create(String.Empty);

                return new JsFunctionResult(result, result);
            }
            else
            {
                // 15.5.2 - When String is called as part of a new expression, it is a constructor: it initialises the newly created object.
                if (parameters.Length > 0)
                    that.Value = parameters[0].ToString();
                else
                    that.Value = String.Empty;

                return new JsFunctionResult(that, that);
            }
        }

        /// <summary>
        /// Used by the String object replace matched pattern
        /// </summary>
        /// <param name="matched"></param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <param name="newString"></param>
        /// <param name="groups"></param>
        /// <returns></returns>
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



        /// <summary>
        /// 15.5.4.2
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static JsInstance ToStringImpl(JsInstance target, JsInstance[] parameters)
        {
            return ValueOfImpl(target, parameters);
        }

        /// <summary>
        /// 15.5.4.3
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ValueOfImpl(JsInstance target, JsInstance[] parameters)
        {
            var jsString = target as JsString;
            if (jsString != null)
                return jsString;

            return JsString.Create((string)target.Value);
        }

        /// <summary>
        /// 15.5.4.4
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance CharAtImpl(JsInstance target, JsInstance[] parameters)
        {
            return SubstringImpl(target, new JsInstance[] { parameters[0], JsNumber.Create(parameters[0].ToNumber() + 1) });
        }

        /// <summary>
        /// 15.5.4.5
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance CharCodeAtImpl(JsInstance target, JsInstance[] parameters)
        {
            var r = target.ToString();
            var at = (int)parameters[0].ToNumber();

            if (r == String.Empty || at > r.Length - 1)
            {
                return JsNumber.NaN;
            }
            else
            {
                return JsNumber.Create(Convert.ToInt32(r[at]));
            }
        }

        /// <summary>
        /// 15.5.3.2
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance FromCharCodeImpl(JsInstance target, JsInstance[] parameters)
        {
            //var r = target.ToString();

            //if (r == String.Empty || at > r.Length - 1)
            //{
            //    return JsNumber.NaN;
            //}
            //else
            //{
            string result = string.Empty;
            foreach (JsInstance arg in parameters)
                result += Convert.ToChar(Convert.ToUInt32(arg.ToNumber()));

            return JsString.Create(result);
            //}
        }

        /// <summary>
        /// 15.5.4.6
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static JsInstance ConcatImpl(JsInstance target, JsInstance[] parameters)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(target.ToString());

            for (int i = 0; i < parameters.Length; i++)
            {
                sb.Append(parameters[i].ToString());
            }

            return JsString.Create(sb.ToString());
        }

        /// <summary>
        /// 15.5.4.7
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static JsInstance IndexOfImpl(JsInstance target, JsInstance[] parameters)
        {
            string source = target.ToString();
            string searchString = parameters[0].ToString();
            int position = parameters.Length > 1 ? (int)parameters[1].ToNumber() : 0;

            if (searchString == String.Empty)
            {
                if (parameters.Length > 1)
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

        /// <summary>
        /// 15.5.4.8
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static JsInstance LastIndexOfImpl(JsInstance target, JsInstance[] parameters)
        {
            string source = target.ToString();
            string searchString = parameters[0].ToString();
            int position = parameters.Length > 1 ? (int)parameters[1].ToNumber() : source.Length;

            return JsNumber.Create(source.Substring(0, position).LastIndexOf(searchString));
        }

        /// <summary>
        /// 15.5.4.9
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance LocaleCompareImpl(JsInstance target, JsInstance[] parameters)
        {
            return JsNumber.Create(target.ToString().CompareTo(parameters[0].ToString()));
        }

        /// <summary>
        /// 15.5.4.10
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance MatchFunc(JsGlobal global, JsInstance target, JsInstance[] parameters)
        {
            JsRegExp regexp = parameters[0].Class == JsInstance.ClassString
                ? global.RegExpClass.New(parameters[0].ToString(), false, false, false)
                : (JsRegExp)parameters[0];

            if (!regexp.IsGlobal)
            {
                return JsRegExpConstructor.ExecImpl(regexp, new JsInstance[] { target });
            }
            else
            {
                var result = global.ArrayClass.New();
                var matches = Regex.Matches(target.ToString(), regexp.Pattern, regexp.Options);
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

        /// <summary>
        /// 15.5.4.11
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ReplaceImpl(JsGlobal global, JsInstance target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
            {
                return JsString.Create(target.ToString());
            }

            JsInstance searchValue = parameters[0];
            JsInstance replaceValue = JsUndefined.Instance;

            if (parameters.Length > 1)
            {
                replaceValue = parameters[1];
            }

            string source = target.ToString();

            JsFunction function = replaceValue as JsFunction;
            if (searchValue.Class == JsInstance.ClassRegexp)
            {
                int count = ((JsRegExp)parameters[0]).IsGlobal ? int.MaxValue : 1;
                var regexp = ((JsRegExp)parameters[0]);
                int lastIndex = regexp.IsGlobal ? 0 : Math.Max(0, (int)regexp["lastIndex"].ToNumber() - 1);

                if (regexp.IsGlobal)
                {
                    regexp["lastIndex"] = JsNumber.Create(0);
                }

                if (replaceValue is JsFunction)
                {
                    string ret = ((JsRegExp)parameters[0]).Regex.Replace(source, delegate(Match m)
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

                        return global.Backend.ExecuteFunction(function, null, replaceParameters.ToArray(), null).ToString();
                    }, count, lastIndex);


                    return JsString.Create(ret);

                }
                else
                {
                    string str = parameters[1].ToString();
                    string ret = ((JsRegExp)parameters[0]).Regex.Replace(target.ToString(), delegate(Match m)
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

                        replaceValue = global.Backend.ExecuteFunction(function, null, replaceParameters.ToArray(), null).Result;

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

        /// <summary>
        /// 15.5.4.12
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SearchImpl(JsGlobal global, JsInstance target, JsInstance[] parameters)
        {
            // Converts the parameters to a regex
            if (parameters[0].Class == JsInstance.ClassString)
            {
                parameters[0] = global.RegExpClass.New(parameters[0].ToString(), false, false, false);
            }

            Match m = ((JsRegExp)parameters[0]).Regex.Match(target.ToString());

            if (m != null && m.Success)
            {
                return JsNumber.Create(m.Index);
            }
            else
            {
                return JsNumber.Create(-1);
            }
        }

        /// <summary>
        /// 15.5.4.13
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SliceImpl(JsInstance target, JsInstance[] parameters)
        {
            string source = target.ToString();
            int start = (int)parameters[0].ToNumber();
            int end = source.Length;
            if (parameters.Length > 1)
            {
                end = (int)parameters[1].ToNumber();
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

        /// <summary>
        /// 15.5.4.14
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SplitImpl(JsGlobal global, JsInstance target, JsInstance[] parameters)
        {
            JsObject a = global.ArrayClass.New();
            string s = target.ToString();

            if (parameters.Length == 0 || IsUndefined(parameters[0]))
            {
                a["0"] = JsString.Create(s);
            }

            JsInstance separator = parameters[0];
            int limit = parameters.Length > 1 ? Convert.ToInt32(parameters[1].ToNumber()) : Int32.MaxValue;
            int length = s.Length;
            string[] result;

            if (separator.Class == JsInstance.ClassRegexp)
            {
                result = ((JsRegExp)parameters[0]).Regex.Split(s, limit);
            }
            else
            {
                result = s.Split(new string[] { separator.ToString() }, limit, StringSplitOptions.None);
            }

            for (int i = 0; i < result.Length; i++)
            {
                a[i.ToString()] = JsString.Create(result[i]);
            }

            return a;
        }

        /// <summary>
        /// 15.5.4.15
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SubstringImpl(JsInstance target, JsInstance[] parameters)
        {
            string str = target.ToString();
            int start = 0, end = str.Length;

            if (parameters.Length > 0 && !double.IsNaN(parameters[0].ToNumber()))
            {
                start = Convert.ToInt32(parameters[0].ToNumber());
            }

            if (parameters.Length > 1 && !IsUndefined(parameters[1]) && !double.IsNaN(parameters[1].ToNumber()))
            {
                end = Convert.ToInt32(parameters[1].ToNumber());
            }

            start = Math.Min(Math.Max(start, 0), Math.Max(0, str.Length - 1));
            end = Math.Min(Math.Max(end, 0), str.Length);
            str = str.Substring(start, end - start);

            return JsString.Create(str);
        }

        public static JsInstance SubstrImpl(JsInstance target, JsInstance[] parameters)
        {
            string str = target.ToString();
            int start = 0, end = str.Length;

            if (parameters.Length > 0 && !double.IsNaN(parameters[0].ToNumber()))
            {
                start = Convert.ToInt32(parameters[0].ToNumber());
            }

            if (parameters.Length > 1 && !IsUndefined(parameters[1]) && !double.IsNaN(parameters[1].ToNumber()))
            {
                end = Convert.ToInt32(parameters[1].ToNumber());
            }

            start = Math.Min(Math.Max(start, 0), Math.Max(0, str.Length - 1));
            end = Math.Min(Math.Max(end, 0), str.Length);
            str = str.Substring(start, end);

            return JsString.Create(str);
        }

        /// <summary>
        /// 15.5.4.16
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ToLowerCaseImpl(JsInstance target, JsInstance[] parameters)
        {
            return JsString.Create(target.ToString().ToLowerInvariant());
        }

        /// <summary>
        /// 15.5.4.17
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ToLocaleLowerCaseImpl(JsInstance target, JsInstance[] parameters)
        {
            return JsString.Create(target.ToString().ToLower());
        }

        /// <summary>
        /// 15.5.4.18
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ToUpperCaseImpl(JsInstance target, JsInstance[] parameters)
        {
            return JsString.Create(target.ToString().ToUpperInvariant());
        }

        /// <summary>
        /// 15.5.4.19
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ToLocaleUpperCaseImpl(JsInstance target, JsInstance[] parameters)
        {
            string str = target.ToString();
            return JsString.Create(str.ToUpper());
        }

        /// <summary>
        /// 15.5.5.1
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static JsInstance LengthImpl(JsInstance target)
        {
            string str = target.ToString();
            return JsNumber.Create(str.Length);
        }
    }
}
