using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Jint.Expressions;
using Jint.Native;
using Jint.Runtime;
using PropertyAttributes = Jint.Native.PropertyAttributes;

namespace Jint.Backend.Dlr
{
    internal class DlrBackend : IJintBackend
    {
        private readonly JintRuntime _runtime;
        private readonly ITypeResolver _typeResolver = CachedTypeResolver.Default;

        public Options Options { get; private set; }

        public JsGlobal Global
        {
            get { return _runtime.Global; }
        }

        public PermissionSet PermissionSet { get; set; }
        public bool AllowClr { get; set; }

        public DlrBackend(Options options, JintEngine engine)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");

            Options = options;
            PermissionSet = new PermissionSet(PermissionState.None);

            _runtime = new JintRuntime(this, Options);
        }

        public object Run(ProgramSyntax program, bool unwrap)
        {
            if (program == null)
                throw new ArgumentNullException("program");

            PrepareTree(program);

            var expression = program.Accept(new ExpressionVisitor());

            PrintExpression(expression);

            EnsureGlobalsDeclared(program);

            var result = ((Func<JintRuntime, JsInstance>)((LambdaExpression)expression).Compile())(_runtime);

            return
                result == null
                ? null
                : unwrap
                    ? Global.Marshaller.MarshalJsValue<object>(result)
                    : result;
        }

        private void EnsureGlobalsDeclared(ProgramSyntax program)
        {
            var scope = Global.GlobalScope;

            foreach (var declaredVariable in program.DeclaredVariables)
            {
                if (
                    declaredVariable.IsDeclared &&
                    !scope.HasOwnProperty(declaredVariable.Name)
                )
                    scope.DefineOwnProperty(declaredVariable.Name, JsUndefined.Instance, PropertyAttributes.DontEnum);
            }
        }

        private void PrepareTree(SyntaxNode node)
        {
            node.Accept(new VariableMarkerPhase(this));
            node.Accept(new TypeMarkerPhase());

            ResetExpressionDump();
        }

        public JsFunction CompileFunction(JsInstance[] parameters)
        {
            if (parameters == null)
                parameters = JsInstance.EmptyArray;

            var newParameters = new List<string>();

            for (int i = 0; i < parameters.Length - 1; i++)
            {
                string arg = parameters[i].ToString();

                foreach (string a in arg.Split(','))
                {
                    newParameters.Add(a.Trim());
                }
            }

            BlockSyntax newBody;

            if (parameters.Length >= 1)
            {
                newBody = JintEngine.CompileBlockStatements(
                    parameters[parameters.Length - 1].Value.ToString()
                );
            }
            else
            {
                newBody = new BlockSyntax(SyntaxNode.EmptyList);
            }

            var function = new FunctionSyntax(null, newParameters, newBody);

            PrepareTree(function);

            return _runtime.CreateFunction(
                function.Name,
                new ExpressionVisitor().DeclareFunction(function),
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
            try
            {
                File.AppendAllText(
                    "Dump.txt",
                    (string)typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(expression, null),
                    Encoding.UTF8
                );
            }
            catch
            {
                // When permissions are set, we may not be able to access the
                // debug view. Just swallow all exceptions here.
            }
        }

        public object CallFunction(string name, object[] args)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            return CallFunction((JsFunction)Global.GlobalScope[name], args);
        }

        public object CallFunction(JsFunction function, object[] args)
        {
            if (function == null)
                throw new ArgumentNullException("function");

            JsInstance[] arguments;

            if (args == null || args.Length == 0)
            {
                arguments = JsInstance.EmptyArray;
            }
            else
            {
                arguments = new JsInstance[args.Length];

                for (int i = 0; i < args.Length; i++)
                {
                    arguments[i] = Global.Marshaller.MarshalClrValue(args[i]);
                }
            }

            var original = new JsInstance[arguments.Length];
            Array.Copy(arguments, original, arguments.Length);

            var result = _runtime.ExecuteFunction(function, JsNull.Instance, arguments, null);

            for (int i = 0; i < args.Length; i++)
            {
                if (!ReferenceEquals(arguments[i], original[i]))
                    args[i] = Global.Marshaller.MarshalJsValue<object>(arguments[i]);
            }

            return Global.Marshaller.MarshalJsValue<object>(result);
        }

        public JsInstance Eval(JsInstance[] arguments)
        {
            if (JsInstance.ClassString != arguments[0].Class)
                return arguments[0];

            ProgramSyntax program;

            try
            {
                program = JintEngine.Compile(arguments[0].ToString());
            }
            catch (Exception e)
            {
                throw new JsException(JsErrorType.SyntaxError, e.Message);
            }

            if (program == null)
                return JsNull.Instance;

            try
            {
                return (JsInstance)Run(program, false);
            }
            catch (Exception e)
            {
                throw new JsException(JsErrorType.EvalError, e.Message);
            }
        }

        public JsInstance ExecuteFunction(JsFunction function, JsInstance that, JsInstance[] arguments, JsInstance[] genericParameters)
        {
            return _runtime.ExecuteFunction(function, that, arguments, genericParameters);
        }

        public int Compare(JsFunction function, JsInstance x, JsInstance y)
        {
            var result = _runtime.ExecuteFunction(
                function,
                JsNull.Instance,
                new[] { x, y },
                null
            );

            return (int)result.ToNumber();
        }

        public JsInstance ResolveUndefined(string typeFullName, Type[] generics)
        {
            if (!AllowClr)
                throw new JsException(JsErrorType.ReferenceError);
            
            if (!String.IsNullOrEmpty(typeFullName))
            {
                EnsureClrAllowed();

                bool haveGenerics = generics != null && generics.Length > 0;

                if (haveGenerics)
                    typeFullName += "`" + generics.Length.ToString(CultureInfo.InvariantCulture);

                var type = _typeResolver.ResolveType(typeFullName);

                if (haveGenerics && type != null)
                    type = type.MakeGenericType(generics);

                if (type != null)
                    return Global.WrapClr(type);
            }

            return new JsUndefined(typeFullName);
        }

        private void EnsureClrAllowed()
        {
            if (!AllowClr)
                throw new SecurityException("Use of Clr is not allowed");
        }
    }
}
