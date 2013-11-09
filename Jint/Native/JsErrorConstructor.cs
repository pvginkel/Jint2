using System;
using System.Collections.Generic;
using System.Text;
using Jint.Expressions;

namespace Jint.Native
{
    [Serializable]
    public class JsErrorConstructor : JsConstructor
    {
        private readonly string _errorType;

        public JsErrorConstructor(IGlobal global, string errorType)
            : base(global)
        {
            _errorType = errorType;
            Name = errorType;

            DefineOwnProperty(PrototypeName, global.ObjectClass.New(this), PropertyAttributes.DontEnum | PropertyAttributes.DontDelete | PropertyAttributes.ReadOnly);
        }

        public override void InitPrototype(IGlobal global)
        {
            //prototype = global.FunctionClass;
            var prototype = PrototypeProperty;

            prototype.DefineOwnProperty("name", global.StringClass.New(_errorType), PropertyAttributes.DontEnum | PropertyAttributes.DontDelete | PropertyAttributes.ReadOnly);
            prototype.DefineOwnProperty("toString", global.FunctionClass.New<JsDictionaryObject>(ToStringImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toLocaleString", global.FunctionClass.New<JsDictionaryObject>(ToStringImpl), PropertyAttributes.DontEnum);
        }

        public JsError New(string message)
        {
            var error = new JsError(Global);
            error["message"] = Global.StringClass.New(message);
            return error;
        }

        public JsError New()
        {
            return New(String.Empty);
        }

        public override JsFunctionResult Execute(IGlobal global, JsDictionaryObject that, JsInstance[] parameters, Type[] genericArguments)
        {
            JsInstance result;

            if (that == null || (that as IGlobal) == global)
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

        public JsInstance ToStringImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            return Global.StringClass.New(target["name"] + ": " + target["message"]);
        }

        public override JsObject Construct(JsInstance[] parameters, Type[] genericArgs, IJintVisitor visitor)
        {
            return parameters != null && parameters.Length > 0 ?
                visitor.Global.ErrorClass.New(parameters[0].ToString()) :
                visitor.Global.ErrorClass.New();
        }
    }
}
