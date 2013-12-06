using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Jint.Native
{
    public partial class JintRuntime
    {
        private readonly JintEngine _engine;

        public JsGlobal Global { get; private set; }
        public JsObject GlobalScope { get; private set; }

        public JintRuntime(JintEngine engine, Options options)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");

            _engine = engine;

            Global = new JsGlobal(this, engine, options);
            GlobalScope = Global.GlobalScope;
        }

        public JsObject CreateFunction(string name, MethodInfo function, object closure, string[] parameters)
        {
            return Global.CreateFunction(
                name,
                (JsFunction)Delegate.CreateDelegate(typeof(JsFunction), function),
                parameters == null ? 0 : parameters.Length,
                closure
            );
        }

        public IEnumerable<JsBox> GetForEachKeys(JsBox obj)
        {
            if (!obj.IsValid)
                yield break;

            if (obj.IsClr)
            {
                var values = obj.ToInstance().Value as IEnumerable;

                if (values != null)
                {
                    foreach (object value in values)
                    {
                        yield return JsBox.CreateObject(Global.WrapClr(value));
                    }

                    yield break;
                }
            }

            foreach (int key in new List<int>(((JsObject)obj).GetKeys()))
            {
                yield return JsString.Box(Global.GetIdentifier(key));
            }
        }

        public JsBox WrapException(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            // TODO: Be smart about how we wrap exceptions. E.g. a cast
            // exception could be converted to a TypeError.

            var type = JsErrorType.Error;

            var jsException = exception as JsException;
            if (jsException != null)
            {
                if (jsException.Value.IsValid)
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

            return JsBox.CreateObject(Global.CreateError(errorClass, exception.Message));
        }

        public JsBox New(JsBox target, JsBox[] arguments, JsBox[] generics)
        {
            if (
                _engine.IsClrAllowed &&
                target.IsUndefined
            )
            {
                var undefined = (JsUndefined)target.ToInstance();
                if (
                    !String.IsNullOrEmpty(undefined.Name) &&
                    generics.Length > 0
                )
                {
                    var genericParameters = new Type[generics.Length];

                    try
                    {
                        for (int i = 0; i < generics.Length; i++)
                        {
                            genericParameters[i] = (Type)generics[i].ToInstance().Value;
                        }
                    }
                    catch (Exception e)
                    {
                        throw new JintException("A type parameter is required", e);
                    }

                    target = _engine.ResolveUndefined(undefined.Name, genericParameters);
                }
            }

            if (!target.IsFunction)
                throw new JsException(JsErrorType.Error, "Function expected.");

            return ((JsObject)target).Construct(this, arguments);
        }

        public int ResolveIdentifier(string name)
        {
            return Global.ResolveIdentifier(name);
        }

        public JsObject CreateArguments(JsObject callee, JsBox[] arguments)
        {
            var result = Global.CreateObject();

            result.SetClass(JsNames.ClassArguments);
            result.SetIsClr(false);

            int length = 0;

            // Add the named parameters.

            if (arguments != null)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    result.DefineOwnProperty(new ValueDescriptor(
                        result,
                        i.ToString(CultureInfo.InvariantCulture),
                        arguments[i],
                        PropertyAttributes.DontEnum
                    ));
                }

                length = arguments.Length;
            }

            result.DefineOwnProperty(new ValueDescriptor(
                result,
                "callee",
                JsBox.CreateObject(callee),
                PropertyAttributes.DontEnum
            ));

            result.DefineOwnProperty(new ValueDescriptor(
                result,
                "length",
                JsNumber.Box(length),
                PropertyAttributes.DontEnum
            ));

            return result;
        }
    }
}
