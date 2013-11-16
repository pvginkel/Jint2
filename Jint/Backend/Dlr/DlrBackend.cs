using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Text;
using Jint.Expressions;
using Jint.Marshal;
using Jint.Native;
using Jint.Runtime;

namespace Jint.Backend.Dlr
{
    internal class DlrBackend : IJintBackend
    {
        private readonly JintContext _context;
        private readonly JintRuntime _runtime;
        private readonly ITypeResolver _typeResolver = CachedTypeResolver.Default;

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

            _runtime = new JintRuntime(this, Options);
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
            return new JsFunctionDelegate(this, _context, func, JsNull.Instance, delegateType).GetDelegate();
        }

        public JsInstance Construct(JsFunction function, JsInstance[] parameters)
        {
            throw new NotImplementedException();
        }

        public JsInstance ResolveUndefined(string typeFullname, Type[] generics)
        {
            if (AllowClr && !String.IsNullOrEmpty(typeFullname))
            {
                EnsureClrAllowed();

                bool haveGenerics = generics != null && generics.Length > 0;

                if (haveGenerics)
                    typeFullname += "`" + generics.Length.ToString(CultureInfo.InvariantCulture);

                var type = _typeResolver.ResolveType(typeFullname);

                if (haveGenerics && type != null)
                    type = type.MakeGenericType(generics);

                if (type != null)
                    return Global.WrapClr(type);
            }

            return new JsUndefined(Global, typeFullname);
        }

        private void EnsureClrAllowed()
        {
            if (!AllowClr)
                throw new SecurityException("Use of Clr is not allowed");
        }
    }
}
