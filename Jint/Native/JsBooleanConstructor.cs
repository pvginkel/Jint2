using System;
using System.Collections.Generic;
using System.Text;
using Jint.Expressions;

namespace Jint.Native
{
    [Serializable]
    public class JsBooleanConstructor : JsConstructor
    {
        public JsBooleanConstructor(JsGlobal global)
            : base(global, BuildPrototype(global))
        {
            Name = "Boolean";
        }

        private static JsObject BuildPrototype(JsGlobal global)
        {
            var prototype = new JsObject(global, global.FunctionClass.Prototype);

            prototype.DefineOwnProperty("toString", global.FunctionClass.New<JsObject>(ToString2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toLocaleString", global.FunctionClass.New<JsObject>(ToString2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("valueOf", global.FunctionClass.New<JsObject>(ValueOfImpl), PropertyAttributes.DontEnum);

            return prototype;
        }

        public static JsInstance ValueOfImpl(JsObject target, JsInstance[] parameters)
        {
            return JsBoolean.Create((bool)target.Value);
        }

        public override JsFunctionResult Execute(JsInstance that, JsInstance[] parameters, Type[] genericArguments)
        {
            JsInstance result;

            // e.g., var foo = Boolean(true);
            if (that == null || that == Global.GlobalScope)
            {
                result = JsBoolean.Create(parameters.Length > 0 && parameters[0].ToBoolean());
            }
            else // e.g., var foo = new Boolean(true);
            {
                if (parameters.Length > 0)
                    that.Value = parameters[0].ToBoolean();
                else
                    that.Value = false;

                result = that;
            }

            return new JsFunctionResult(result, that);
        }


        public static JsInstance ToString2(JsObject target, JsInstance[] parameters)
        {
            return JsString.Create(target.ToString());
        }
    }
}
