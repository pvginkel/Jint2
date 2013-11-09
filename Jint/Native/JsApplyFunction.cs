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
    public class JsApplyFunction : JsFunction
    {
        public JsApplyFunction(JsFunctionConstructor constructor)
            : base(constructor.PrototypeProperty)
        {
            DefineOwnProperty("length", constructor.Global.NumberClass.New(2), PropertyAttributes.ReadOnly);
        }

        public override JsFunctionResult Execute(IGlobal global, JsDictionaryObject that, JsInstance[] parameters, Type[] genericArguments)
        {
            throw new NotImplementedException();
            /*
            JsFunction function = that as JsFunction;

            if (function == null)
            {
                throw new ArgumentException("the target of call() must be a function");
            }
            JsDictionaryObject @this;
            JsInstance[] targetParameters;
            if (parameters.Length >= 1 && parameters[0] != JsUndefined.Instance && parameters[0] != JsNull.Instance)
                @this = parameters[0] as JsDictionaryObject;
            else
                @this = visitor.Global as JsDictionaryObject;

            if (parameters.Length >= 2 && parameters[1] != JsNull.Instance)
            {
                JsObject arguments = parameters[1] as JsObject;
                if (arguments == null)
                    throw new JsException(visitor.Global.TypeErrorClass.New("second argument must be an array"));
                targetParameters = new JsInstance[arguments.Length];
                for (int i = 0; i < arguments.Length; i++)
                {
                    targetParameters[i] = arguments[i.ToString()];
                }
            }
            else
            {
                targetParameters = JsInstance.Empty;
            }


            // Executes the statements in 'that' and use @this as the target of the call
            visitor.ExecuteFunction(function, @this, targetParameters);
            return visitor.Result;
            //visitor.CallFunction(function, @this, targetParameters);

            //return visitor.Result;
             */
        }
    }
}
