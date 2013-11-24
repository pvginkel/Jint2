using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Text;
using Jint.Backend.Dlr;
using Jint.Expressions;
using Jint.Native;

namespace Jint.Runtime
{
    public partial class JintRuntime
    {
        private readonly IJintBackend _backend;
        private readonly Options _options;
        private readonly JsFunctionConstructor _functionClass;
        private readonly JsErrorConstructor _errorClass;
        private readonly JsErrorConstructor _typeErrorClass;

        public JsGlobal Global { get; private set; }
        public JsObject GlobalScope { get; private set; }

        public JintRuntime(IJintBackend backend, Options options)
        {
            if (backend == null)
                throw new ArgumentNullException("backend");

            _backend = backend;
            _options = options;

            Global = new JsGlobal(backend, options);
            GlobalScope = Global.GlobalScope;

            _functionClass = Global.FunctionClass;
            _errorClass = Global.ErrorClass;
            _typeErrorClass = Global.TypeErrorClass;
        }

        public JsFunction CreateFunction(string name, DlrFunctionDelegate function, object closure, string[] parameters)
        {
            return new DlrFunction(Global, function, new JsObject(Global, _functionClass.Prototype), closure, this)
            {
                Name = name,
                Arguments = new List<string>(parameters ?? new string[0])
            };
        }

        public JsInstance ExecuteFunction(JsInstance that, JsInstance target, JsInstance[] parameters, JsInstance[] genericArguments)
        {
            Type[] genericParameters = null;

            if (_backend.AllowClr && genericArguments != null && genericArguments.Length > 0)
            {
                genericParameters = new Type[genericArguments.Length];

                try
                {
                    for (int i = 0; i < genericArguments.Length; i++)
                    {
                        genericParameters[i] = (Type)genericArguments[i].Value;
                    }
                }
                catch (Exception e)
                {
                    throw new JintException("A type parameter is required", e);
                }
            }

            var function = target as JsFunction;
            if (function == null)
                throw new JsException(_errorClass.New("Function expected."));

            var result = ExecuteFunctionCore(
                function,
                that,
                parameters ?? JsInstance.EmptyArray,
                genericParameters
            );

            return result.Result;
        }

        public JsFunctionResult ExecuteFunctionCore(JsFunction function, JsInstance that, JsInstance[] parameters, Type[] genericParameters)
        {
            if (function == null)
                throw new ArgumentNullException("function");

            try
            {
                if (_backend.AllowClr)
                    _backend.PermissionSet.PermitOnly();

                if (!_backend.AllowClr)
                    genericParameters = null;

                return function.Execute(that ?? Global.GlobalScope, parameters ?? JsInstance.EmptyArray, genericParameters);
            }
            finally
            {
                if (_backend.AllowClr)
                    CodeAccessPermission.RevertPermitOnly();
            }
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
                foreach (string key in new List<string>(((JsObject)obj).GetKeys()))
                {
                    yield return JsString.Create(key);
                }
            }
        }

        public JsInstance WrapException(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            var jsException =
                exception as JsException ??
                new JsException(_errorClass.New(exception.Message));

            return jsException.Value;
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
                throw new JsException(_errorClass.New("Function expected."));

            return function.Construct(arguments, null);
        }
    }
}
