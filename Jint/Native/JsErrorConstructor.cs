using System;
using System.Collections.Generic;
using System.Text;
using Jint.Expressions;

namespace Jint.Native
{
    [Serializable]
    public class JsErrorConstructor : JsConstructor
    {
        public JsErrorConstructor(JsGlobal global, string errorType)
            : base(global, BuildPrototype(global, errorType))
        {
            Name = errorType;
        }

        private static JsObject BuildPrototype(JsGlobal global, string errorType)
        {
            var prototype = new JsObject(global, global.FunctionClass.Prototype);

            prototype.DefineOwnProperty("name", JsString.Create(errorType), PropertyAttributes.DontEnum | PropertyAttributes.DontDelete | PropertyAttributes.ReadOnly);
            prototype.DefineOwnProperty("toString", global.FunctionClass.New<JsObject>(ToStringImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toLocaleString", global.FunctionClass.New<JsObject>(ToStringImpl), PropertyAttributes.DontEnum);

            return prototype;
        }

        public JsError New(string message)
        {
            var error = new JsError(Global, Prototype);
            error["message"] = JsString.Create(message);
            return error;
        }

        public JsError New()
        {
            return New(String.Empty);
        }

        public override JsFunctionResult Execute(JsInstance that, JsInstance[] parameters, Type[] genericArguments)
        {
            JsInstance result;

            if (that == null || that == Global.GlobalScope)
            {
                result = parameters.Length > 0 ? New(parameters[0].ToString()) : New();
            }
            else
            {
                if (parameters.Length > 0)
                {
                    that.Value = parameters[0].ToString();
                }
                else
                {
                    that.Value = String.Empty;
                }

                result = that;
            }

            return new JsFunctionResult(result, that);
        }

        public static JsInstance ToStringImpl(JsObject target, JsInstance[] parameters)
        {
            return JsString.Create(target["name"] + ": " + target["message"]);
        }

        public override JsObject Construct(JsInstance[] parameters, Type[] genericArgs)
        {
            return parameters != null && parameters.Length > 0 ?
                Global.ErrorClass.New(parameters[0].ToString()) :
                Global.ErrorClass.New();
        }
    }
}
