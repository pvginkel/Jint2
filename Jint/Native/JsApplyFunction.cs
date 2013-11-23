using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Jint.Expressions;

namespace Jint.Native
{
    /// <summary>
    /// This class is used to model the function.call behaviour, which takes two arguments: this, and the parameters
    /// It is defined in Function.prototype so that every function can use it by default
    /// </summary>
    [Serializable]
    public class JsApplyFunction : JsFunction
    {
        public JsApplyFunction(JsFunctionConstructor constructor)
            : base(constructor.PrototypeProperty)
        {
            DefineOwnProperty("length", constructor.Global.NumberClass.New(2), PropertyAttributes.ReadOnly);
        }

        public override JsFunctionResult Execute(JsGlobal global, JsDictionaryObject that, JsInstance[] parameters, Type[] genericArguments)
        {
            var function = that as JsFunction;

            if (function == null)
                throw new ArgumentException("The target of call() must be a function");

            JsDictionaryObject @this;

            if (parameters.Length >= 1 && !(parameters[0] is JsUndefined) && parameters[0] != JsNull.Instance)
                @this = parameters[0] as JsDictionaryObject;
            else
                @this = global as JsDictionaryObject;

            JsInstance[] parametersCopy;

            if (parameters.Length >= 2 && parameters[1] != JsNull.Instance)
            {
                JsObject arguments = parameters[1] as JsObject;
                if (arguments == null)
                    throw new JsException(global.TypeErrorClass.New("Second argument must be an array"));

                parametersCopy = new JsInstance[arguments.Length];

                for (int i = 0; i < arguments.Length; i++)
                {
                    parametersCopy[i] = arguments[i.ToString(CultureInfo.InvariantCulture)];
                }
            }
            else
            {
                parametersCopy = Empty;
            }

            // Executes the statements in 'that' and use _this as the target of the call
            return global.Backend.ExecuteFunction(function, @this, parametersCopy, null);
        }
    }
}
