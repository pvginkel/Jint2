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
        public JsCallFunction(JsObject prototype)
            : base(prototype)
        {
            DefineOwnProperty("length", JsNumber.Create(1), PropertyAttributes.ReadOnly);
        }

        public override JsFunctionResult Execute(JsGlobal global, JsInstance that, JsInstance[] parameters, Type[] genericArguments)
        {
            var function = that as JsFunction;

            if (function == null)
                throw new ArgumentException("the target of call() must be a function");

            JsDictionaryObject @this;

            if (parameters.Length >= 1 && !(parameters[0] is JsUndefined) && parameters[0] != JsNull.Instance)
                @this = parameters[0] as JsDictionaryObject;
            else
                @this = global as JsDictionaryObject;

            JsInstance[] parametersCopy;

            if (parameters.Length >= 2 && parameters[1] != JsNull.Instance)
            {
                parametersCopy = new JsInstance[parameters.Length - 1];
                Array.Copy(parameters, 1, parametersCopy, 0, parametersCopy.Length);
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
