using System;
using System.Collections.Generic;
using System.Text;
using Jint.Expressions;

namespace Jint.Native
{
    [Serializable]
    public class JsBooleanConstructor : JsConstructor
    {
        public JsBoolean False { get; private set; }
        public JsBoolean True { get; private set; }

        public JsBooleanConstructor(JsGlobal global)
            : base(global)
        {
            Name = "Boolean";

            DefineOwnProperty(PrototypeName, global.ObjectClass.New(this), PropertyAttributes.DontEnum | PropertyAttributes.DontDelete | PropertyAttributes.ReadOnly);

            True = new JsBoolean(true, PrototypeProperty);
            False = new JsBoolean(false, PrototypeProperty);
        }

        public override void InitPrototype(JsGlobal global)
        {
            var prototype = PrototypeProperty;

            prototype.DefineOwnProperty("toString", global.FunctionClass.New<JsDictionaryObject>(ToString2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toLocaleString", global.FunctionClass.New<JsDictionaryObject>(ToString2), PropertyAttributes.DontEnum);
        }

        public JsBoolean New()
        {
            return New(false);
        }

        public JsBoolean New(bool value)
        {
            return value ? True : False;
        }

        public override JsFunctionResult Execute(JsGlobal global, JsDictionaryObject that, JsInstance[] parameters, Type[] genericArguments)
        {
            JsInstance result;

            // e.g., var foo = Boolean(true);
            if (that == null || (that as JsGlobal) == global)
            {
                result = parameters.Length > 0 ? new JsBoolean(parameters[0].ToBoolean(), PrototypeProperty) : new JsBoolean(PrototypeProperty);
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


        public JsInstance ToString2(JsDictionaryObject target, JsInstance[] parameters)
        {
            return Global.StringClass.New(target.ToString());
        }
    }
}
