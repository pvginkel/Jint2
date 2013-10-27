using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using System.Text;
using Jint.Delegates;
using Jint.Expressions;
using Jint.Marshal;
using Jint.Native;

namespace Jint.Backend.Interpreted
{
    [Serializable]
    internal class InterpretedBackend : IJintBackend
    {
        private readonly Visitor _visitor;

        public IGlobal Global
        {
            get { return _visitor.Global; }
        }

        public JsScope GlobalScope
        {
            get { return _visitor.GlobalScope; }
        }

        public bool AllowClr
        {
            get { return _visitor.AllowClr; }
            set { _visitor.AllowClr = value; }
        }

        public PermissionSet PermissionSet
        {
            get { return _visitor.PermissionSet; }
            set { _visitor.PermissionSet = value; }
        }

        public InterpretedBackend(Options options)
        {
            _visitor = new Visitor(options, this);

            var global = _visitor.Global as JsObject;

            global["ToBoolean"] = _visitor.Global.FunctionClass.New(new Func<object, Boolean>(Convert.ToBoolean));
            global["ToByte"] = _visitor.Global.FunctionClass.New(new Func<object, Byte>(Convert.ToByte));
            global["ToChar"] = _visitor.Global.FunctionClass.New(new Func<object, Char>(Convert.ToChar));
            global["ToDateTime"] = _visitor.Global.FunctionClass.New(new Func<object, DateTime>(Convert.ToDateTime));
            global["ToDecimal"] = _visitor.Global.FunctionClass.New(new Func<object, Decimal>(Convert.ToDecimal));
            global["ToDouble"] = _visitor.Global.FunctionClass.New(new Func<object, Double>(Convert.ToDouble));
            global["ToInt16"] = _visitor.Global.FunctionClass.New(new Func<object, Int16>(Convert.ToInt16));
            global["ToInt32"] = _visitor.Global.FunctionClass.New(new Func<object, Int32>(Convert.ToInt32));
            global["ToInt64"] = _visitor.Global.FunctionClass.New(new Func<object, Int64>(Convert.ToInt64));
            global["ToSByte"] = _visitor.Global.FunctionClass.New(new Func<object, SByte>(Convert.ToSByte));
            global["ToSingle"] = _visitor.Global.FunctionClass.New(new Func<object, Single>(Convert.ToSingle));
            global["ToString"] = _visitor.Global.FunctionClass.New(new Func<object, String>(Convert.ToString));
            global["ToUInt16"] = _visitor.Global.FunctionClass.New(new Func<object, UInt16>(Convert.ToUInt16));
            global["ToUInt32"] = _visitor.Global.FunctionClass.New(new Func<object, UInt32>(Convert.ToUInt32));
            global["ToUInt64"] = _visitor.Global.FunctionClass.New(new Func<object, UInt64>(Convert.ToUInt64));
        }

        public object Run(Program program, bool unwrap)
        {
            if (program == null)
                throw new ArgumentNullException("program");

            _visitor.Result = null;

            try
            {
#if DEBUG
                var stopwatch = Stopwatch.StartNew();
#endif

                _visitor.Visit(program);

#if DEBUG
                Console.WriteLine(stopwatch.Elapsed);
#endif
            }
            catch (SecurityException)
            {
                throw;
            }
            catch (JsException e)
            {
                string message = e.Message;

                if (e.Value is JsError)
                    message = e.Value.Value.ToString();

                var stackTrace = new StringBuilder();
                var source = String.Empty;

                if (_visitor.CurrentStatement.Source != null)
                {
                    source =
                        Environment.NewLine +
                        _visitor.CurrentStatement.Source + Environment.NewLine +
                        _visitor.CurrentStatement.Source.Code;
                }

                throw new JintException(message + source + stackTrace, e);
            }
            catch (Exception e)
            {
                var stackTrace = new StringBuilder();
                string source = String.Empty;

                if (_visitor.CurrentStatement != null && _visitor.CurrentStatement.Source != null)
                {
                    source =
                        Environment.NewLine +
                        _visitor.CurrentStatement.Source + Environment.NewLine +
                        _visitor.CurrentStatement.Source.Code;
                }

                throw new JintException(e.Message + source + stackTrace, e);
            }

            return
                _visitor.Result == null
                ? null
                : unwrap
                    ? _visitor.Global.Marshaller.MarshalJsValue<object>(_visitor.Result)
                    : _visitor.Result;
        }

        public object CallFunction(string name, object[] args)
        {
            var oldResult = _visitor.Result;

            _visitor.Visit(new Identifier(name));

            var returnValue = CallFunction((JsFunction)_visitor.Result, args);

            _visitor.Result = oldResult;

            return returnValue;
        }

        public object CallFunction(JsFunction function, object[] args)
        {
            _visitor.ExecuteFunction(function, null, Array.ConvertAll(args, x => _visitor.Global.Marshaller.MarshalClrValue(x)));

            return _visitor.Global.Marshaller.MarshalJsValue<object>(_visitor.Returned);
        }

        public JsInstance ExecuteFunction(JsFunction function, JsDictionaryObject that, JsInstance[] arguments, Type[] genericParameters)
        {
            _visitor.ExecuteFunction(function, that, arguments, genericParameters);

            return _visitor.Returned;
        }

        public int Compare(JsFunction function, JsInstance x, JsInstance y)
        {
            _visitor.Result = function;

            var methodCall = new MethodCall(
                new List<Expression>
                {
                    new ValueExpression(x, TypeCode.Object),
                    new ValueExpression(y, TypeCode.Object)
                }
            );

            methodCall.Accept(_visitor);

            return Math.Sign(_visitor.Result.ToNumber());
        }

        public object MarshalJsFunctionHelper(JsFunction func, Type delegateType)
        {
            // create independent visitor
            var visitor = new Visitor(Global, new JsScope((JsObject)Global))
            {
                AllowClr = _visitor.AllowClr,
                PermissionSet = _visitor.PermissionSet
            };

            JsFunctionDelegate wrapper = new JsFunctionDelegate(visitor, func, JsNull.Instance, delegateType);

            return wrapper.GetDelegate();
        }

        public JsInstance Construct(JsFunction function, JsInstance[] parameters)
        {
            throw new NotImplementedException();
        }

        public JsInstance Eval(JsInstance[] arguments)
        {
            Program program;

            try
            {
                program = JintEngine.Compile(arguments[0].ToString());
            }
            catch (Exception e)
            {
                throw new JsException(_visitor.Global.SyntaxErrorClass.New(e.Message));
            }

            try
            {
                program.Accept(_visitor);
            }
            catch (Exception e)
            {
                throw new JsException(_visitor.Global.EvalErrorClass.New(e.Message));
            }

            return _visitor.Result;
        }
    }
}
