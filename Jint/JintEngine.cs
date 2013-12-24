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
using Antlr.Runtime;
using Jint.Bound;
using Jint.Compiler;
using Jint.Expressions;
using Jint.Native;
using Jint.Native.Interop;
using Jint.Parser;
using PropertyAttributes = Jint.Native.PropertyAttributes;
using TypeMarkerPhase = Jint.Bound.TypeMarkerPhase;

namespace Jint
{
    public class JintEngine
    {
        private readonly JintRuntime _runtime;
        private readonly ITypeResolver _typeResolver = CachedTypeResolver.Default;

        public Options Options { get; private set; }
        public bool IsClrAllowed { get; set; }

        public JintEngine(Options options)
        {
            Options = options;
            PermissionSet = new PermissionSet(PermissionState.None);

            _runtime = new JintRuntime(this, Options);

            ResetExpressionDump();
        }

        public PermissionSet PermissionSet { get; private set; }

        /// <summary>
        /// A global object associated with this engine instance
        /// </summary>
        public JsGlobal Global
        {
            get { return _runtime.Global; }
        }

        [DebuggerStepThrough]
        public JintEngine()
            : this(Options.EcmaScript5 | Options.Strict)
        {
        }

        public JintEngine AllowClr()
        {
            return AllowClr(true);
        }

        public JintEngine AllowClr(bool allowed)
        {
            IsClrAllowed = true;

            return this;
        }

        internal static ProgramSyntax Compile(string source)
        {
            if (String.IsNullOrEmpty(source))
                return null;

            var lexer = new ES3Lexer(new ANTLRStringStream(source));
            var parser = new ES3Parser(new CommonTokenStream(lexer), source);

            var program = parser.Execute();

            if (parser.Errors != null && parser.Errors.Count > 0)
                throw new JintException(String.Join(Environment.NewLine, parser.Errors.ToArray()));

            return program;
        }

        internal static BodySyntax CompileBlockStatements(string source)
        {
            BodySyntax block = null;
            if (!string.IsNullOrEmpty(source))
            {
                var lexer = new ES3Lexer(new ANTLRStringStream(source));
                var parser = new ES3Parser(new CommonTokenStream(lexer), source);

                block = parser.ExecuteBlockStatements();

                if (parser.Errors != null && parser.Errors.Count > 0)
                    throw new JintException(String.Join(Environment.NewLine, parser.Errors.ToArray()));
            }

            return block;
        }

        /// <summary>
        /// Pre-compiles the expression in order to check syntax errors.
        /// If errors are detected, the Error property contains the message.
        /// </summary>
        /// <returns>True if the expression syntax is correct, otherwise False</returns>
        public static bool HasErrors(string script, out string errors)
        {
            try
            {
                errors = null;
                ProgramSyntax program = Compile(script);

                // In case HasErrors() is called multiple times for the same expression
                return program != null;
            }
            catch (Exception e)
            {
                errors = e.Message;
                return true;
            }
        }

        /// <summary>
        /// Runs a set of JavaScript statements and optionally returns a value if return is called
        /// </summary>
        /// <param name="script">The script to execute</param>
        /// <returns>Optionally, returns a value from the scripts</returns>
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.Security.SecurityException" />
        /// <exception cref="Jint.JintException" />
        public object Run(string script)
        {
            return Run(script, null);
        }

        /// <summary>
        /// Runs a set of JavaScript statements and optionally returns a value if return is called
        /// </summary>
        /// <param name="script">The script to execute</param>
        /// <returns>Optionally, returns a value from the scripts</returns>
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.Security.SecurityException" />
        /// <exception cref="Jint.JintException" />
        public object Run(string script, string fileName)
        {
            return Run(script, true, fileName);
        }

        /// <summary>
        /// Runs a set of JavaScript statements and optionally returns a value if return is called
        /// </summary>
        /// <param name="program">The expression tree to execute</param>
        /// <returns>Optionally, returns a value from the scripts</returns>
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.Security.SecurityException" />
        /// <exception cref="Jint.JintException" />
        internal object Run(ProgramSyntax program, string fileName)
        {
            return Run(program, true, fileName);
        }

        /// <summary>
        /// Runs a set of JavaScript statements and optionally returns a value if return is called
        /// </summary>
        /// <param name="reader">The TextReader to read script from</param>
        /// <returns>Optionally, returns a value from the scripts</returns>
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.Security.SecurityException" />
        /// <exception cref="Jint.JintException" />
        public object Run(TextReader reader, string fileName)
        {
            return Run(reader.ReadToEnd(), fileName);
        }

        /// <summary>
        /// Runs a set of JavaScript statements and optionally returns a value if return is called
        /// </summary>
        /// <param name="reader">The TextReader to read script from</param>
        /// <param name="unwrap">Whether to unwrap the returned value to a CLR instance. <value>True</value> by default.</param>
        /// <returns>Optionally, returns a value from the scripts</returns>
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.Security.SecurityException" />
        /// <exception cref="Jint.JintException" />
        public object Run(TextReader reader, bool unwrap, string fileName)
        {
            return Run(reader.ReadToEnd(), unwrap, fileName);
        }

        /// <summary>
        /// Runs a set of JavaScript statements and optionally returns a value if return is called
        /// </summary>
        /// <param name="script">The script to execute</param>
        /// <param name="unwrap">Whether to unwrap the returned value to a CLR instance. <value>True</value> by default.</param>
        /// <returns>Optionally, returns a value from the scripts</returns>
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.Security.SecurityException" />
        /// <exception cref="Jint.JintException" />
        public object Run(string script, bool unwrap)
        {
            return Run(script, unwrap, null);
        }

        /// <summary>
        /// Runs a set of JavaScript statements and optionally returns a value if return is called
        /// </summary>
        /// <param name="script">The script to execute</param>
        /// <param name="unwrap">Whether to unwrap the returned value to a CLR instance. <value>True</value> by default.</param>
        /// <returns>Optionally, returns a value from the scripts</returns>
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.Security.SecurityException" />
        /// <exception cref="Jint.JintException" />
        public object Run(string script, bool unwrap, string fileName)
        {
            if (script == null)
                throw new
                    ArgumentException("Script can't be null", "script");

            ProgramSyntax program;

            try
            {
                program = Compile(script);
            }
            catch (Exception e)
            {
                throw new JintException("An unexpected error occurred while parsing the script", e);
            }

            if (program == null)
                return null;

            return Run(program, unwrap, fileName);
        }

        /// <summary>
        /// Runs a set of JavaScript statements and optionally returns a value if return is called
        /// </summary>
        /// <param name="program">The expression tree to execute</param>
        /// <param name="unwrap">Whether to unwrap the returned value to a CLR instance. <value>True</value> by default.</param>
        /// <returns>Optionally, returns a value from the scripts</returns>
        internal object Run(ProgramSyntax program, bool unwrap, string fileName)
        {
            // Don't wrap exceptions while debugging to make stack traces
            // easier to read.

#if !DEBUG
            try
            {
#endif
                return CompileAndRun(program, unwrap, fileName);
#if !DEBUG
            }
            catch (SecurityException)
            {
                throw;
            }
            catch (JsException e)
            {
                throw new JintException(e.Message, e);
            }
            catch (Exception e)
            {
                throw new JintException(e.Message, e);
            }
#endif
        }

        #region SetParameter overloads

        /// <summary>
        /// Defines an external object to be available inside the script
        /// </summary>
        /// <param name="name">Local name of the object during the execution of the script</param>
        /// <param name="value">Available object</param>
        /// <returns>The current JintEngine instance</returns>
        public JintEngine SetParameter(string name, object value)
        {
            Global.GlobalScope.SetProperty(name, Global.WrapClr(value));

            return this;
        }

        /// <summary>
        /// Defines an external Double value to be available inside the script
        /// </summary>
        /// <param name="name">Local name of the Double value during the execution of the script</param>
        /// <param name="value">Available Double value</param>
        /// <returns>The current JintEngine instance</returns>
        public JintEngine SetParameter(string name, double value)
        {
            Global.GlobalScope.SetProperty(name, value);

            return this;
        }

        /// <summary>
        /// Defines an external String instance to be available inside the script
        /// </summary>
        /// <param name="name">Local name of the String instance during the execution of the script</param>
        /// <param name="value">Available String instance</param>
        /// <returns>The current JintEngine instance</returns>
        public JintEngine SetParameter(string name, string value)
        {
            if (value == null)
                Global.GlobalScope.SetProperty(name, JsNull.Instance);
            else
                Global.GlobalScope.SetProperty(name, value);

            return this;
        }

        /// <summary>
        /// Defines an external Int32 value to be available inside the script
        /// </summary>
        /// <param name="name">Local name of the Int32 value during the execution of the script</param>
        /// <param name="value">Available Int32 value</param>
        /// <returns>The current JintEngine instance</returns>
        public JintEngine SetParameter(string name, int value)
        {
            Global.GlobalScope.SetProperty(name, Global.WrapClr(value));
            return this;
        }

        /// <summary>
        /// Defines an external Boolean value to be available inside the script
        /// </summary>
        /// <param name="name">Local name of the Boolean value during the execution of the script</param>
        /// <param name="value">Available Boolean value</param>
        /// <returns>The current JintEngine instance</returns>
        public JintEngine SetParameter(string name, bool value)
        {
            Global.GlobalScope.SetProperty(name, BooleanBoxes.Box(value));

            return this;
        }

        /// <summary>
        /// Defines an external DateTime value to be available inside the script
        /// </summary>
        /// <param name="name">Local name of the DateTime value during the execution of the script</param>
        /// <param name="value">Available DateTime value</param>
        /// <returns>The current JintEngine instance</returns>
        public JintEngine SetParameter(string name, DateTime value)
        {
            Global.GlobalScope.SetProperty(name, Global.CreateDate(value));

            return this;
        }
        #endregion

        public JintEngine AddPermission(IPermission perm)
        {
            PermissionSet.AddPermission(perm);

            return this;
        }

        public JintEngine SetFunction(string name, JsObject function)
        {
            Global.GlobalScope.SetProperty(name, function);

            return this;
        }

        public JintEngine SetFunction(string name, Delegate @delegate)
        {
            if (@delegate == null)
                throw new ArgumentNullException();

            Global.GlobalScope.SetProperty(
                name,
                ProxyHelper.BuildDelegateFunction(
                    Global,
                    @delegate
                )
            );

            return this;
        }

        /// <summary>
        /// Escapes a JavaScript string literal
        /// </summary>
        /// <param name="value">The string literal to escape</param>
        /// <returns>The escaped string literal, without single quotes, back slashes and line breaks</returns>
        public static string EscapeStringLiteral(string value)
        {
            return value.Replace("\\", "\\\\").Replace("'", "\\'").Replace(Environment.NewLine, "\\r\\n");
        }

        public JintEngine DisableSecurity()
        {
            PermissionSet = new PermissionSet(PermissionState.Unrestricted);
            return this;
        }

        public JintEngine EnableSecurity()
        {
            PermissionSet = new PermissionSet(PermissionState.None);
            return this;
        }

        private object CompileAndRun(ProgramSyntax program, bool unwrap, string fileName)
        {
            if (program == null)
                throw new ArgumentNullException("program");

            object result;

            if (program.IsLiteral)
            {
                // If the whole program is a literal, there's no use in invoking
                // the compiler.

                result = Global.BuildLiteral(program);
            }
            else
            {
                PrepareTree(program);

                var bindingVisitor = new BindingVisitor();

                program.Accept(bindingVisitor);

                var boundProgram = SquelchPhase.Perform(bindingVisitor.Program);
                DefiniteAssignmentPhase.Perform(boundProgram);
                TypeMarkerPhase.Perform(boundProgram);

                PrintBound(boundProgram);

                EnsureGlobalsDeclared(boundProgram);

                var method = new CodeGenerator(this, fileName).BuildMainMethod(boundProgram);

                result = method(_runtime);
            }

            return
                result == null
                ? null
                : unwrap
                    ? Global.Marshaller.MarshalJsValue<object>(result)
                    : result;
        }

        [Conditional("DEBUG")]
        private void PrintBound(BoundProgram program)
        {
            var functions = FunctionGatherer.Gather(program);

            var bodies = new List<BoundBody> { program.Body };
            bodies.AddRange(functions.Select(p => p.Body));

            using (var writer = File.CreateText("Bound Dump.txt"))
            {
                for (int i = 0; i < bodies.Count; i++)
                {
                    if (i == 0)
                        writer.WriteLine("Program:");
                    else
                        writer.WriteLine("Function:");

                    writer.WriteLine();
                    new BoundTreePrettyPrintVisitor(writer).Visit(bodies[i]);
                    writer.WriteLine();
                }
            }
        }

        private void EnsureGlobalsDeclared(BoundProgram program)
        {
            var scope = Global.GlobalScope;

            foreach (var local in program.Body.Locals)
            {
                if (
                    local.IsDeclared &&
                    !scope.HasOwnProperty(local.Name)
                )
                    scope.DefineProperty(local.Name, JsUndefined.Instance, PropertyAttributes.DontEnum);
            }
        }

        private void PrepareTree(SyntaxNode node)
        {
            node.Accept(new VariableMarkerPhase(this));
        }

        internal JsObject CompileFunction(object[] parameters)
        {
            if (parameters == null)
                parameters = JsValue.EmptyArray;

            var newParameters = new List<string>();

            for (int i = 0; i < parameters.Length - 1; i++)
            {
                string arg = JsValue.ToString(parameters[i]);

                foreach (string a in arg.Split(','))
                {
                    newParameters.Add(a.Trim());
                }
            }

            BodySyntax newBody;
            string sourceCode = null;

            if (parameters.Length >= 1)
            {
                sourceCode = JsValue.ToString(parameters[parameters.Length - 1]);
                newBody = CompileBlockStatements(sourceCode);
            }
            else
            {
                newBody = new BodySyntax(BodyType.Function, SyntaxNode.EmptyList, new VariableCollection());
            }

            var function = new FunctionSyntax(null, newParameters, newBody, null, null);

            PrepareTree(function);

            var bindingVisitor = new BindingVisitor();

            var boundFunction = bindingVisitor.DeclareFunction(function);

            boundFunction = SquelchPhase.Perform(boundFunction);
            DefiniteAssignmentPhase.Perform(boundFunction);
            TypeMarkerPhase.Perform(boundFunction);

            return _runtime.CreateFunction(
                function.Name,
                new CodeGenerator(this, null).BuildFunction(boundFunction),
                null,
                function.Parameters.ToArray(),
                sourceCode
            );
        }

        [Conditional("DEBUG")]
        private static void ResetExpressionDump()
        {
            File.WriteAllText("Dump.txt", "");
        }

        [Conditional("DEBUG")]
        internal static void PrintExpression(Expression expression)
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

        public object CallFunction(string name, params object[] arguments)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            return CallFunction((JsObject)Global.GlobalScope.GetProperty(name), arguments);
        }

        public object CallFunction(JsObject function, params object[] arguments)
        {
            if (function == null)
                throw new ArgumentNullException("function");

            object[] argumentsCopy;

            if (arguments == null || arguments.Length == 0)
            {
                argumentsCopy = JsValue.EmptyArray;
            }
            else
            {
                argumentsCopy = new object[arguments.Length];

                for (int i = 0; i < arguments.Length; i++)
                {
                    argumentsCopy[i] = Global.Marshaller.MarshalClrValue(arguments[i]);
                }
            }

            var original = new object[argumentsCopy.Length];
            Array.Copy(argumentsCopy, original, argumentsCopy.Length);

            var result = function.Execute(_runtime, JsNull.Instance, argumentsCopy);

            for (int i = 0; i < arguments.Length; i++)
            {
                arguments[i] = Global.Marshaller.MarshalJsValue<object>(argumentsCopy[i]);
            }

            return Global.Marshaller.MarshalJsValue<object>(result);
        }

        internal object Eval(object[] arguments)
        {
            if (!(arguments[0] is string))
                return arguments[0];

            ProgramSyntax program;

            try
            {
                program = Compile((string)arguments[0]);
            }
            catch (Exception e)
            {
                throw new JsException(JsErrorType.SyntaxError, e.Message);
            }

            if (program == null)
                return JsNull.Instance;

            try
            {
                return Run(program, false, null);
            }
            catch (Exception e)
            {
                throw new JsException(JsErrorType.EvalError, e.Message);
            }
        }

        internal int Compare(JsObject function, object x, object y)
        {
            var result = function.Execute(
                _runtime,
                JsNull.Instance,
                new[] { x, y }
            );

            return (int)JsValue.ToNumber(result);
        }

        internal object ResolveUndefined(string typeFullName, Type[] generics)
        {
            if (!IsClrAllowed)
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
            if (!IsClrAllowed)
                throw new SecurityException("Use of Clr is not allowed");
        }
    }
}
