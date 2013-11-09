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
        public JsRegExpConstructor(IGlobal global)
            : base(global)
        {
            Name = "RegExp";
            DefineOwnProperty(PrototypeName, global.ObjectClass.New(this), PropertyAttributes.DontDelete | PropertyAttributes.DontEnum | PropertyAttributes.ReadOnly);
        }

        public override void InitPrototype(IGlobal global)
        {
            var prototype = PrototypeProperty;
            //prototype = global.ObjectClass.New(this);

            prototype.DefineOwnProperty("toString", global.FunctionClass.New<JsDictionaryObject>(ToStringImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toLocaleString", global.FunctionClass.New<JsDictionaryObject>(ToStringImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("lastIndex", global.FunctionClass.New<JsRegExp>(GetLastIndex), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("exec", global.FunctionClass.New<JsRegExp>(ExecImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("test", global.FunctionClass.New<JsRegExp>(TestImpl), PropertyAttributes.DontEnum);
        }

        public JsInstance GetLastIndex(JsRegExp regex, JsInstance[] parameters)
        {
            return regex["lastIndex"];
        }

        public JsRegExp New()
        {
            return New(String.Empty, false, false, false);
        }

        public JsRegExp New(string pattern, bool g, bool i, bool m)
        {
            var ret = new JsRegExp(pattern, g, i, m, PrototypeProperty);
            ret["source"] = Global.StringClass.New(pattern);
            ret["lastIndex"] = Global.NumberClass.New(0);
            ret["global"] = Global.BooleanClass.New(g);

            return ret;
        }

        public JsInstance ExecImpl(JsRegExp regexp, JsInstance[] parameters)
        {
            JsArray a = Global.ArrayClass.New();
            string input = parameters[0].ToString();
            a["input"] = Global.StringClass.New(input);

            int i = 0;
            var lastIndex = regexp.IsGlobal ? regexp["lastIndex"].ToNumber() : 0;
            MatchCollection matches = Regex.Matches(input.Substring((int)lastIndex), regexp.Pattern, regexp.Options);
            if (matches.Count > 0)
            {
                // A[Global.NumberClass.New(i++)] = Global.StringClass.New(matches[0].Value);
                a["index"] = Global.NumberClass.New(matches[0].Index);

                if (regexp.IsGlobal)
                {
                    regexp["lastIndex"] = Global.NumberClass.New(lastIndex + matches[0].Index + matches[0].Value.Length);
                }

                foreach (Group g in matches[0].Groups)
                {
                    a[Global.NumberClass.New(i++)] = Global.StringClass.New(g.Value);
                }

                return a;
            }
            else
            {
                return JsNull.Instance;
            }

        }

        public JsInstance TestImpl(JsRegExp regex, JsInstance[] parameters)
        {
            var array = ExecImpl(regex, parameters) as JsArray;
            return Global.BooleanClass.New(array != null && array.Length > 0);
        }

        public override JsFunctionResult Execute(IGlobal globa, JsDictionaryObject that, JsInstance[] parameters, Type[] genericArguments)
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

        public JsInstance ToStringImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            return Global.StringClass.New(target.ToString());
        }
    }
}
