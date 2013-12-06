﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Jint.Compiler;
using Jint.Expressions;
using Antlr.Runtime;
using Jint.Native;
using System.Security;
using System.Security.Permissions;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Jint.Native.Interop;
using Jint.Parser;
using ExpressionVisitor = Jint.Compiler.ExpressionVisitor;
using PropertyAttributes = Jint.Native.PropertyAttributes;

namespace Jint
{
    [Serializable]
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

        public static ProgramSyntax Compile(string source)
        {
            ProgramSyntax program = null;
            if (!string.IsNullOrEmpty(source))
            {
                var lexer = new ES3Lexer(new ANTLRStringStream(source));
                var parser = new ES3Parser(new CommonTokenStream(lexer));

                program = parser.Execute();

                if (parser.Errors != null && parser.Errors.Count > 0)
                    throw new JintException(String.Join(Environment.NewLine, parser.Errors.ToArray()));
            }

            return program;
        }

        internal static BlockSyntax CompileBlockStatements(string source)
        {
            BlockSyntax block = null;
            if (!string.IsNullOrEmpty(source))
            {
                var lexer = new ES3Lexer(new ANTLRStringStream(source));
                var parser = new ES3Parser(new CommonTokenStream(lexer));

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
            return Run(script, true);
        }

        /// <summary>
        /// Runs a set of JavaScript statements and optionally returns a value if return is called
        /// </summary>
        /// <param name="program">The expression tree to execute</param>
        /// <returns>Optionally, returns a value from the scripts</returns>
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.Security.SecurityException" />
        /// <exception cref="Jint.JintException" />
        public object Run(ProgramSyntax program)
        {
            return Run(program, true);
        }

        /// <summary>
        /// Runs a set of JavaScript statements and optionally returns a value if return is called
        /// </summary>
        /// <param name="reader">The TextReader to read script from</param>
        /// <returns>Optionally, returns a value from the scripts</returns>
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.Security.SecurityException" />
        /// <exception cref="Jint.JintException" />
        public object Run(TextReader reader)
        {
            return Run(reader.ReadToEnd());
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
        public object Run(TextReader reader, bool unwrap)
        {
            return Run(reader.ReadToEnd(), unwrap);
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

            return Run(program, unwrap);
        }

        /// <summary>
        /// Runs a set of JavaScript statements and optionally returns a value if return is called
        /// </summary>
        /// <param name="program">The expression tree to execute</param>
        /// <param name="unwrap">Whether to unwrap the returned value to a CLR instance. <value>True</value> by default.</param>
        /// <returns>Optionally, returns a value from the scripts</returns>
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.Security.SecurityException" />
        /// <exception cref="Jint.JintException" />
        public object Run(ProgramSyntax program, bool unwrap)
        {
            // Don't wrap exceptions while debugging to make stack traces
            // easier to read.

#if !DEBUG
            try
            {
#endif
                return CompileAndRun(program, unwrap);
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
            Global.GlobalScope[name] = JsBox.CreateObject(Global.WrapClr(value));

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
            Global.GlobalScope[name] = JsBox.CreateNumber(value);

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
                Global.GlobalScope[name] = JsBox.Null;
            else
                Global.GlobalScope[name] = JsString.Box(value);

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
            Global.GlobalScope[name] = JsBox.CreateObject(Global.WrapClr(value));
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
            Global.GlobalScope[name] = JsBox.CreateBoolean(value);

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
            Global.GlobalScope[name] = JsBox.CreateObject(Global.CreateDate(value));

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
            Global.GlobalScope[name] = JsBox.CreateObject(function);

            return this;
        }

        public JintEngine SetFunction(string name, Delegate @delegate)
        {
            if (@delegate == null)
                throw new ArgumentNullException();

            Global.GlobalScope[name] = JsBox.CreateObject(ProxyHelper.BuildDelegateFunction(
                Global,
                @delegate
            ));

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

        public void Save(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            new BinaryFormatter().Serialize(stream, this);
        }

        public static JintEngine Load(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            return (JintEngine)new BinaryFormatter().Deserialize(stream);
        }

        private object CompileAndRun(ProgramSyntax program, bool unwrap)
        {
            if (program == null)
                throw new ArgumentNullException("program");

            PrepareTree(program);

            JsBox result;

            if (program.IsLiteral)
            {
                // If the whole program is a literal, there's no use in invoking
                // the compiler.

                result = Global.BuildLiteral(program);
            }
            else
            {
                var expression = program.Accept(new ExpressionVisitor(Global));

                PrintExpression(expression);

                EnsureGlobalsDeclared(program);

                result = ((Func<JintRuntime, JsBox>)((LambdaExpression)expression).Compile())(_runtime);
            }

            return
                !result.IsValid
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
                    !scope.HasOwnProperty(declaredVariable.Index)
                )
                    scope.DefineOwnProperty(declaredVariable.Name, JsBox.Undefined, PropertyAttributes.DontEnum);
            }
        }

        private void PrepareTree(SyntaxNode node)
        {
            node.Accept(new VariableMarkerPhase(this));
            node.Accept(new TypeMarkerPhase());
        }

        internal JsObject CompileFunction(JsBox[] parameters)
        {
            if (parameters == null)
                parameters = JsBox.EmptyArray;

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
                newBody = CompileBlockStatements(
                    parameters[parameters.Length - 1].ToString()
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
                new ExpressionVisitor(Global).DeclareFunction(function),
                null,
                function.Parameters.ToArray()
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

        public object CallFunction(string name, params object[] args)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            return CallFunction((JsObject)Global.GlobalScope[name], args);
        }

        public object CallFunction(JsObject function, params object[] args)
        {
            if (function == null)
                throw new ArgumentNullException("function");

            JsBox[] arguments;

            if (args == null || args.Length == 0)
            {
                arguments = JsBox.EmptyArray;
            }
            else
            {
                arguments = new JsBox[args.Length];

                for (int i = 0; i < args.Length; i++)
                {
                    arguments[i] = Global.Marshaller.MarshalClrValue(args[i]);
                }
            }

            var original = new JsBox[arguments.Length];
            Array.Copy(arguments, original, arguments.Length);

            var result = function.Execute(_runtime, JsBox.Null, arguments, null);

            for (int i = 0; i < args.Length; i++)
            {
                args[i] = Global.Marshaller.MarshalJsValue<object>(arguments[i]);
            }

            return Global.Marshaller.MarshalJsValue<object>(result);
        }

        internal JsBox Eval(JsBox[] arguments)
        {
            if (JsNames.ClassString != arguments[0].GetClass())
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
                return JsBox.Null;

            try
            {
                return (JsBox)Run(program, false);
            }
            catch (Exception e)
            {
                throw new JsException(JsErrorType.EvalError, e.Message);
            }
        }

        internal int Compare(JsObject function, JsBox x, JsBox y)
        {
            var result = function.Execute(
                _runtime,
                JsBox.Null,
                new[] { x, y },
                null
            );

            return (int)result.ToNumber();
        }

        internal JsBox ResolveUndefined(string typeFullName, Type[] generics)
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
                    return JsBox.CreateObject(Global.WrapClr(type));
            }

            return JsBox.CreateUndefined(typeFullName);
        }

        private void EnsureClrAllowed()
        {
            if (!IsClrAllowed)
                throw new SecurityException("Use of Clr is not allowed");
        }
    }
}
