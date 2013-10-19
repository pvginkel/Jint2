using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Jint.Delegates;
using Jint.Expressions;
using Jint.Marshal;
using Jint.Native;

namespace Jint.Backend.Interpreted
{
    [Serializable]
    internal class InterpretedBackend : IBackend
    {
        private readonly ExecutionVisitor _visitor;

        public IGlobal Global
        {
            get { return _visitor.Global; }
        }

        public JsScope GlobalScope
        {
            get { return _visitor.GlobalScope; }
        }

        public bool AllowClr { get; set; }

        public PermissionSet PermissionSet { get; set; }

        public InterpretedBackend(Options options)
        {
            _visitor = new ExecutionVisitor(options, this);
            PermissionSet = new PermissionSet(PermissionState.None);
            _visitor.AllowClr = AllowClr;

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
                throw new ArgumentException("Script can't be null", "program");

            _visitor.PermissionSet = PermissionSet;
            _visitor.AllowClr = AllowClr;
            _visitor.Result = null;

            try
            {
                _visitor.Visit(program);
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
                    source = Environment.NewLine + _visitor.CurrentStatement.Source.ToString()
                            + Environment.NewLine + _visitor.CurrentStatement.Source.Code;
                }

                throw new JintException(message + source + stackTrace, e);
            }
            catch (Exception e)
            {
                StringBuilder stackTrace = new StringBuilder();
                string source = String.Empty;

                if (_visitor.CurrentStatement != null && _visitor.CurrentStatement.Source != null)
                {
                    source = Environment.NewLine + _visitor.CurrentStatement.Source.ToString()
                            + Environment.NewLine + _visitor.CurrentStatement.Source.Code;
                }

                throw new JintException(e.Message + source + stackTrace, e);
            }

            return _visitor.Result == null ? null : unwrap ? _visitor.Global.Marshaller.MarshalJsValue<object>(_visitor.Result) : _visitor.Result;
        }

        public object CallFunction(string name, object[] args)
        {
            JsInstance oldResult = _visitor.Result;
            _visitor.Visit(new Identifier(name));
            var returnValue = CallFunction((JsFunction)_visitor.Result, args);
            _visitor.Result = oldResult;
            return returnValue;
        }

        public object CallFunction(JsFunction function, object[] args)
        {
            _visitor.ExecuteFunction(function, null, Array.ConvertAll(args, x => _visitor.Global.Marshaller.MarshalClrValue<object>(x)));
            return _visitor.Global.Marshaller.MarshalJsValue<object>(_visitor.Returned);
        }

        public JsInstance ExecuteFunction(JsFunction function, JsDictionaryObject that, JsInstance[] arguments)
        {
            _visitor.ExecuteFunction(function, that, arguments);

            return _visitor.Returned;
        }

        public int Compare(JsFunction function, JsInstance x, JsInstance y)
        {
            _visitor.Result = function;
            new MethodCall(new List<Expression>() { new ValueExpression(x, TypeCode.Object), new ValueExpression(y, TypeCode.Object) }).Accept((IStatementVisitor)_visitor);
            return Math.Sign(_visitor.Result.ToNumber());
        }

        public object MarshalJsFunctionHelper(JsFunction func, Type delegateType)
        {
            // create independent visitor
            var visitor = new ExecutionVisitor(Global, new JsScope((JsObject)Global))
            {
                AllowClr = _visitor.AllowClr,
                PermissionSet = _visitor.PermissionSet
            };

            JsFunctionDelegate wrapper = new JsFunctionDelegate(visitor, func, JsNull.Instance, delegateType);

            return wrapper.GetDelegate();
        }

        public JsInstance Eval(JsInstance[] arguments)
        {
            Program p;

            try
            {
                p = JintEngine.Compile(arguments[0].ToString());
            }
            catch (Exception e)
            {
                throw new JsException(_visitor.Global.SyntaxErrorClass.New(e.Message));
            }

            try
            {
                p.Accept(_visitor);
            }
            catch (Exception e)
            {
                throw new JsException(_visitor.Global.EvalErrorClass.New(e.Message));
            }

            return _visitor.Result;
        }
    }
}
