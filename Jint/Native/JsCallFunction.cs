using System;
using System.Collections.Generic;
using System.Text;
using Jint.Expressions;

namespace Jint.Native
{
    /// <summary>
    /// This class is used to model the function.call behaviour, which takes two arguments: this, and the parameters
    /// It is defined in Function.prototype so that every function can use it by default
    /// </summary>
    [Serializable]
    public class JsCallFunction : JsFunction
    {
        public JsCallFunction(JsGlobal global, JsObject prototype)
            : base(global, prototype)
        {
            DefineOwnProperty("length", JsNumber.Create(1), PropertyAttributes.ReadOnly);
        }

        public override JsFunctionResult Execute(JsInstance that, JsInstance[] parameters, Type[] genericArguments)
        {
            var function = that as JsFunction;

            if (function == null)
                throw new ArgumentException("the target of call() must be a function");

            JsObject @this;

            if (parameters.Length >= 1 && !IsNullOrUndefined(parameters[0]))
                @this = parameters[0] as JsObject;
            else
                @this = Global.GlobalScope;

            JsInstance[] parametersCopy;

            if (parameters.Length >= 2 && !IsNull(parameters[1]))
            {
                parametersCopy = new JsInstance[parameters.Length - 1];
                Array.Copy(parameters, 1, parametersCopy, 0, parametersCopy.Length);
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
