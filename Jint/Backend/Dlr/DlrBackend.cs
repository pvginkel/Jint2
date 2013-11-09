using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Text;
using Jint.Expressions;
using Jint.Native;
using Jint.Runtime;

namespace Jint.Backend.Dlr
{
    internal class DlrBackend : IJintBackend
    {
        private readonly JintContext _context;
        private readonly JintRuntime _runtime;

        public Options Options { get; private set; }

        public IGlobal Global
        {
            get { return _runtime.Global; }
        }

        public JsScope GlobalScope
        {
            get { return _runtime.GlobalScope; }
        }

        public PermissionSet PermissionSet { get; set; }
        public bool AllowClr { get; set; }

        public DlrBackend(Options options)
        {
            Options = options;

            _runtime = new JintRuntime(this, Options, AllowClr, PermissionSet);
            _context = new JintContext(_runtime.Global);
        }

        public object Run(ProgramSyntax program, bool unwrap)
        {
            if (program == null)
                throw new ArgumentNullException("program");

            program.Accept(new VariableMarkerPhase(this));

            var expression = program.Accept(new ExpressionVisitor(_context));

            PrintExpression(expression);   

            var result = ((Func<JintRuntime, JsInstance>)((LambdaExpression)expression).Compile())(_runtime);

            return
                result == null
                ? null
                : unwrap
                    ? Global.Marshaller.MarshalJsValue<object>(result)
                    : result;
        }

        [Conditional("DEBUG")]
        public static void PrintExpression(Expression expression)
        {
            Console.WriteLine(
                typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(expression, null)
            );
        }

        public object CallFunction(string name, object[] args)
        {
            throw new NotImplementedException();
        }

        public object CallFunction(JsFunction function, object[] args)
        {
            throw new NotImplementedException();
        }

        public JsInstance Eval(JsInstance[] arguments)
        {
            throw new NotImplementedException();
        }

        public JsInstance ExecuteFunction(JsFunction function, JsDictionaryObject that, JsInstance[] arguments, Type[] genericParameters)
        {
            throw new NotImplementedException();
        }

        public int Compare(JsFunction function, JsInstance x, JsInstance y)
        {
            throw new NotImplementedException();
        }

        public object MarshalJsFunctionHelper(JsFunction func, Type delegateType)
        {
            throw new NotImplementedException();
        }

        public JsInstance Construct(JsFunction function, JsInstance[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
