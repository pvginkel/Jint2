using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

            ResetExpressionDump();

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

        public JsFunction CompileFunction(JsInstance[] parameters, Type[] genericArgs)
        {
            var function = new FunctionSyntax();

            for (int i = 0; i < parameters.Length - 1; i++)
            {
                string arg = parameters[i].ToString();

                foreach (string a in arg.Split(','))
                {
                    function.Parameters.Add(a.Trim());
                }
            }

            if (parameters.Length >= 1)
                function.Body = JintEngine.CompileBlockStatements(parameters[parameters.Length - 1].Value.ToString());

            function.Accept(new VariableMarkerPhase(this));

            ResetExpressionDump();

            return _runtime.CreateFunction(
                function.Name,
                new ExpressionVisitor(_context).DeclareFunction(function),
                null,
                function.Parameters.ToArray()
            );
        }

        [Conditional("DEBUG")]
        public static void ResetExpressionDump()
        {
            File.WriteAllText("Dump.txt", "");
        }

        [Conditional("DEBUG")]
        public static void PrintExpression(Expression expression)
        {
            File.AppendAllText(
                "Dump.txt",
                (string)typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(expression, null)
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

        public JsFunctionResult ExecuteFunction(JsFunction function, JsDictionaryObject that, JsInstance[] arguments, Type[] genericParameters)
        {
            return _runtime.ExecuteFunctionCore(function, that, arguments, genericParameters);
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
