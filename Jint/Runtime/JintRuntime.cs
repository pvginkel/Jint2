using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using Jint.Native;

namespace Jint.Runtime
{
    public partial class JintRuntime
    {
        private readonly IJintBackend _backend;

        public JsGlobal Global { get; private set; }
        public JsObject GlobalScope { get; private set; }

        public JintRuntime(IJintBackend backend, Options options)
        {
            if (backend == null)
                throw new ArgumentNullException("backend");

            _backend = backend;

            Global = new JsGlobal(this, backend, options);
            GlobalScope = Global.GlobalScope;
        }

        public JsFunction CreateFunction(string name, JsFunctionDelegate function, object closure, string[] parameters)
        {
            return Global.CreateFunction(
                name,
                function,
                parameters == null ? 0 : parameters.Length,
                closure
            );
        }

        public JsInstance ExecuteFunction(JsFunction function, JsInstance that, JsInstance[] parameters, JsInstance[] genericArguments)
        {
            if (function == null)
                throw new ArgumentNullException("function");

            return function.Execute(
                this,
                that ?? Global.GlobalScope,
                parameters ?? JsInstance.EmptyArray,
                genericArguments
            );
        }

        public IEnumerable<JsInstance> GetForEachKeys(JsInstance obj)
        {
            if (obj == null)
                yield break;

            var values = obj.Value as IEnumerable;

            if (values != null)
            {
                foreach (object value in values)
                {
                    yield return Global.WrapClr(value);
                }
            }
            else
            {
                foreach (int key in new List<int>(((JsObject)obj).GetKeys()))
                {
                    yield return JsString.Create(Global.GetIdentifier(key));
                }
            }
        }

        public JsInstance WrapException(Exception exception)
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

            return Global.CreateError(errorClass.Prototype, exception.Message);
        }

        public JsInstance New(JsInstance target, JsInstance[] arguments, JsInstance[] generics)
        {
            var undefined = target as JsUndefined;

            if (_backend.AllowClr && undefined != null && !String.IsNullOrEmpty(undefined.Name) && generics.Length > 0)
            {
                var genericParameters = new Type[generics.Length];

                try
                {
                    for (int i = 0; i < generics.Length; i++)
                    {
                        genericParameters[i] = (Type)generics[i].Value;
                    }
                }
                catch (Exception e)
                {
                    throw new JintException("A type parameter is required", e);
                }

                target = _backend.ResolveUndefined(undefined.Name, genericParameters);
            }

            var function = target as JsFunction;
            if (function == null)
                throw new JsException(JsErrorType.Error, "Function expected.");

            return function.Construct(this, arguments);
        }
    }
}
