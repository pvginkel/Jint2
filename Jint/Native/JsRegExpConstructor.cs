using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Jint.Expressions;

namespace Jint.Native
{
    [Serializable]
    public class JsRegExpConstructor : JsConstructor
    {
        public JsRegExpConstructor(JsGlobal global)
            : base(global, BuildPrototype(global))
        {
            Name = "RegExp";
        }

        private static JsObject BuildPrototype(JsGlobal global)
        {
            var prototype = new JsObject(global, global.FunctionClass.Prototype);

            prototype.DefineOwnProperty("toString", global.FunctionClass.New<JsObject>(ToStringImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toLocaleString", global.FunctionClass.New<JsObject>(ToStringImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("lastIndex", global.FunctionClass.New<JsRegExp>(GetLastIndex), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("exec", global.FunctionClass.New<JsRegExp>(ExecImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("test", global.FunctionClass.New<JsRegExp>(TestImpl), PropertyAttributes.DontEnum);

            return prototype;
        }

        public static JsInstance GetLastIndex(JsRegExp regex, JsInstance[] parameters)
        {
            return regex["lastIndex"];
        }

        public JsRegExp New()
        {
            return New(String.Empty, false, false, false);
        }

        public JsRegExp New(string pattern, bool g, bool i, bool m)
        {
            var ret = new JsRegExp(Global, pattern, g, i, m, Prototype);
            ret["source"] = JsString.Create(pattern);
            ret["lastIndex"] = JsNumber.Create(0);
            ret["global"] = JsBoolean.Create(g);

            return ret;
        }

        public static JsInstance ExecImpl(JsRegExp regexp, JsInstance[] parameters)
        {
            JsArray a = regexp.Global.ArrayClass.New();
            string input = parameters[0].ToString();
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

        public static JsInstance TestImpl(JsRegExp regex, JsInstance[] parameters)
        {
            var array = ExecImpl(regex, parameters) as JsArray;
            return JsBoolean.Create(array != null && array.Length > 0);
        }

        public override JsFunctionResult Execute(JsInstance that, JsInstance[] parameters, Type[] genericArguments)
        {
            JsInstance result;

            if (parameters.Length == 0)
            {
                result = New();
                return new JsFunctionResult(result, result);
                //throw new ArgumentNullException("pattern");
            }

            bool g = false, m = false, ic = false;

            if (parameters.Length == 2)
            {
                string strParam = parameters[1].ToString();
                if (strParam != null)
                {
                    m = strParam.Contains("m");
                    ic = strParam.Contains("i");
                    g = strParam.Contains("g");
                }
            }

            result = New(parameters[0].ToString(), g, ic, m);
            return new JsFunctionResult(result, result);
        }

        public static JsInstance ToStringImpl(JsObject target, JsInstance[] parameters)
        {
            return JsString.Create(target.ToString());
        }
    }
}
