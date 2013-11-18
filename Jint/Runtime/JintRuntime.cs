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
        internal const string GlobalName = "Global";
        internal const string GlobalScopeName = "GlobalScope";

        private readonly IJintBackend _backend;
        private readonly Options _options;
        private readonly JsFunctionConstructor _functionClass;
        private readonly JsErrorConstructor _errorClass;
        private readonly JsErrorConstructor _typeErrorClass;
        private readonly JsObjectConstructor _objectClass;
        private readonly JsNumberConstructor _numberClass;
        private readonly JsBooleanConstructor _booleanClass;
        private readonly JsStringConstructor _stringClass;

        public JsScope GlobalScope { get; private set; }
        public JsGlobal Global { get; private set; }

        public JintRuntime(IJintBackend backend, Options options)
        {
            if (backend == null)
                throw new ArgumentNullException("backend");

            _backend = backend;
            _options = options;

            var global = new JsGlobal(backend, options);

            Global = global;
            GlobalScope = new JsScope(global);

            _functionClass = Global.FunctionClass;
            _errorClass = Global.ErrorClass;
            _typeErrorClass = Global.TypeErrorClass;
            _objectClass = Global.ObjectClass;
            _numberClass = Global.NumberClass;
            _booleanClass = Global.BooleanClass;
            _stringClass = Global.StringClass;

            global["ToBoolean"] = _functionClass.New(new Func<object, Boolean>(Convert.ToBoolean));
            global["ToByte"] = _functionClass.New(new Func<object, Byte>(Convert.ToByte));
            global["ToChar"] = _functionClass.New(new Func<object, Char>(Convert.ToChar));
            global["ToDateTime"] = _functionClass.New(new Func<object, DateTime>(Convert.ToDateTime));
            global["ToDecimal"] = _functionClass.New(new Func<object, Decimal>(Convert.ToDecimal));
            global["ToDouble"] = _functionClass.New(new Func<object, Double>(Convert.ToDouble));
            global["ToInt16"] = _functionClass.New(new Func<object, Int16>(Convert.ToInt16));
            global["ToInt32"] = _functionClass.New(new Func<object, Int32>(Convert.ToInt32));
            global["ToInt64"] = _functionClass.New(new Func<object, Int64>(Convert.ToInt64));
            global["ToSByte"] = _functionClass.New(new Func<object, SByte>(Convert.ToSByte));
            global["ToSingle"] = _functionClass.New(new Func<object, Single>(Convert.ToSingle));
            global["ToString"] = _functionClass.New(new Func<object, String>(Convert.ToString));
            global["ToUInt16"] = _functionClass.New(new Func<object, UInt16>(Convert.ToUInt16));
            global["ToUInt32"] = _functionClass.New(new Func<object, UInt32>(Convert.ToUInt32));
            global["ToUInt64"] = _functionClass.New(new Func<object, UInt64>(Convert.ToUInt64));
        }

        public JsFunction CreateFunction(string name, DlrFunctionDelegate function, object closure, string[] parameters)
        {
            var result = new DlrFunction(function, _functionClass.PrototypeProperty, closure, this)
            {
                Name = name,
                Arguments = new List<string>(parameters ?? new string[0])
            };

            result.PrototypeProperty = _objectClass.New(result);

            return result;
        }

        public JsInstance ExecuteFunction(JsInstance that, JsInstance target, JsInstance[] parameters, JsInstance[] genericArguments, out bool[] outParameters)
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
                (JsObject)that,
                parameters ?? JsInstance.Empty,
                genericParameters
            );

            outParameters = result.OutParameters;

            return result.Result;
        }

        public JsFunctionResult ExecuteFunctionCore(JsFunction function, JsDictionaryObject that, JsInstance[] parameters, Type[] genericParameters)
        {
            /*
            if (function == null)
                return null;

            // ecma chapter 10.
            // TODO: move creation of the activation object to the JsFunction
            // create new argument object and instantinate arguments into it
            var args = new JsArguments(Global, function, parameters);

            // create new activation object and copy instantinated arguments to it
            // Activation should be before the function.Scope hierarchy
            var functionScope = new JsScope(function.Scope ?? GlobalScope);

            for (int i = 0; i < function.Arguments.Count; i++)
            {
                if (i < parameters.Length)
                {
                    functionScope.DefineOwnProperty(
                        new LinkedDescriptor(
                            functionScope,
                            function.Arguments[i],
                            args.GetDescriptor(i.ToString()),
                            args
                        )
                    );
                }
                else
                {
                    functionScope.DefineOwnProperty(
                        new ValueDescriptor(
                            functionScope,
                            function.Arguments[i],
                            JsUndefined.Instance
                        )
                    );
                }
            }

            // define arguments variable
            if (_isStrict)
                functionScope.DefineOwnProperty(JsScope.Arguments, args);
            else
                args.DefineOwnProperty(JsScope.Arguments, args);

            if (that == null)
                that = Global;

            functionScope.DefineOwnProperty(JsScope.This, that);

            try
            {
                if (_backend.AllowClr)
                    _backend.PermissionSet.PermitOnly();

                //var previousScope = _program.EnterScope(functionScope);

                try
                {
                    if (!_backend.AllowClr || (genericParameters != null && genericParameters.Length == 0))
                        genericParameters = null;

                    var result = function.Execute(Global, that, parameters);

                    return result.Result;
                }
                finally
                {
                    //_program.ExitScope(previousScope);
                }
            }
            finally
            {
                if (_backend.AllowClr)
                    CodeAccessPermission.RevertPermitOnly();
            }
            */
            if (function == null)
                throw new ArgumentNullException("function");

            try
            {
                if (_backend.AllowClr)
                    _backend.PermissionSet.PermitOnly();

                if (!_backend.AllowClr)
                    genericParameters = null;

                return function.Execute(Global, that ?? Global, parameters ?? JsInstance.Empty, genericParameters);
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
                foreach (string key in new List<string>(((JsDictionaryObject)obj).GetKeys()))
                {
                    yield return _stringClass.New(key);
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

            return function.Construct(arguments, null, Global);
        }
    }
}
