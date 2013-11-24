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
        public JsApplyFunction(JsGlobal global, JsObject prototype)
            : base(global, prototype)
        {
            DefineOwnProperty("length", JsNumber.Create(2), PropertyAttributes.ReadOnly);
        }

        public override JsFunctionResult Execute(JsInstance that, JsInstance[] parameters, Type[] genericArguments)
        {
            var function = that as JsFunction;

            if (function == null)
                throw new ArgumentException("The target of call() must be a function");

            JsInstance @this;

            if (parameters.Length >= 1 && !IsNullOrUndefined(parameters[0]))
                @this = parameters[0];
            else
                @this = Global.GlobalScope;

            JsInstance[] parametersCopy;

            if (parameters.Length >= 2 && !IsNull(parameters[1]))
            {
                JsObject arguments = parameters[1] as JsObject;
                if (arguments == null)
                    throw new JsException(Global.TypeErrorClass.New("Second argument must be an array"));

                parametersCopy = new JsInstance[arguments.Length];

                for (int i = 0; i < arguments.Length; i++)
                {
                    parametersCopy[i] = arguments[i.ToString(CultureInfo.InvariantCulture)];
                }
            }
            else
            {
                parametersCopy = EmptyArray;
            }

            // Executes the statements in 'that' and use _this as the target of the call
            return Global.Backend.ExecuteFunction(function, @this, parametersCopy, null);
        }
    }
}
