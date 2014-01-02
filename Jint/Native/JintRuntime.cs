using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    public partial class JintRuntime
    {
        private readonly JintEngine _engine;

        public JsGlobal Global { get; private set; }
        public JsObject GlobalScope { get; private set; }

        public JintRuntime(JintEngine engine)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");

            _engine = engine;

            Global = new JsGlobal(this, engine);
            GlobalScope = Global.GlobalScope;
        }

        public JsObject CreateFunction(string name, JsFunction function, int parameterCount)
        {
            return Global.CreateFunction(
                name,
                function,
                parameterCount
            );
        }

        public object[] GetForEachKeys(object obj)
        {
            if (obj == null)
                return new object[0];

            var result = new List<object>();

            if (JsValue.IsClr(obj))
            {
                var values = JsValue.UnwrapValue(obj) as IEnumerable;

                if (values != null)
                {
                    foreach (object value in values)
                    {
                        result.Add(Global.WrapClr(value));
                    }

                    return result.ToArray();
                }
            }

            foreach (int key in ((JsObject)obj).GetKeys())
            {
                result.Add(Global.GetIdentifier(key));
            }

            return result.ToArray();
        }

        public object WrapException(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            // TODO: Be smart about how we wrap exceptions. E.g. a cast
            // exception could be converted to a TypeError.

            var type = JsErrorType.Error;

            var jsException = exception as JsException;
            if (jsException != null)
            {
                if (jsException.Value != null)
                    return jsException.Value;

                type = jsException.Type;
            }

            JsObject errorClass;

            switch (type)
            {
                case JsErrorType.EvalError: errorClass = Global.EvalErrorClass; break;
                case JsErrorType.RangeError: errorClass = Global.RangeErrorClass; break;
                case JsErrorType.ReferenceError: errorClass = Global.ReferenceErrorClass; break;
                case JsErrorType.SyntaxError: errorClass = Global.SyntaxErrorClass; break;
                case JsErrorType.TypeError: errorClass = Global.TypeErrorClass; break;
                case JsErrorType.URIError: errorClass = Global.URIErrorClass; break;
                default: errorClass = Global.ErrorClass; break;
            }

            return Global.CreateError(errorClass, exception.Message);
        }

        public object New(object target, object[] arguments)
        {
            if (
                _engine.IsClrAllowed &&
                JsValue.IsUndefined(target)
            )
            {
                var genericArguments = ExtractGenericArguments(ref arguments);

                var undefined = (JsUndefined)target;
                if (
                    !String.IsNullOrEmpty(undefined.Name) &&
                    genericArguments != null &&
                    genericArguments.Length > 0
                )
                {
                    var genericParameters = new Type[genericArguments.Length];

                    try
                    {
                        for (int i = 0; i < genericArguments.Length; i++)
                        {
                            genericParameters[i] = (Type)JsValue.UnwrapValue(genericArguments[i]);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new JintException("A type parameter is required", e);
                    }

                    target = _engine.ResolveUndefined(undefined.Name, genericParameters);
                }
            }

            if (!JsValue.IsFunction(target))
                throw new JsException(JsErrorType.Error, "Function expected.");

            return ((JsObject)target).Construct(this, arguments);
        }

        internal static object[] ExtractGenericArguments(ref object[] arguments)
        {
            if (arguments.Length > 0)
            {
                var genericArguments = arguments[arguments.Length - 1] as JsGenericArguments;
                if (genericArguments != null)
                {
                    Array.Resize(ref arguments, arguments.Length - 1);
                    return genericArguments.Arguments;
                }
            }

            return null;
        }

        public int ResolveIdentifier(string name)
        {
            return Global.ResolveIdentifier(name);
        }

        public JsObject CreateArguments(JsObject callee, object[] arguments)
        {
            var result = Global.CreateObject();

            result.SetClass(JsNames.ClassArguments);
            result.IsClr = false;

            int length = 0;

            // Add the named parameters.

            if (arguments != null)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    result.DefineProperty(
                        i,
                        arguments[i],
                        PropertyAttributes.DontEnum
                    );
                }

                length = arguments.Length;
            }

            result.DefineProperty(
                Id.callee,
                callee,
                PropertyAttributes.DontEnum
            );

            result.DefineProperty(
                Id.length,
                (double)length,
                PropertyAttributes.DontEnum
            );

            return result;
        }
    }
}
