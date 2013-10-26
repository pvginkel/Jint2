using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using CSharpSyntax.Printer;
using Jint.Expressions;
using Jint.Native;
using Microsoft.CSharp;
using Syntax = CSharpSyntax.Syntax;

namespace Jint.Backend.Compiled
{
    internal class CompiledBackend : IJintBackend
    {
        private readonly Options _options;
        private readonly Visitor _visitor;
        private JintProgram _program;

        public IGlobal Global { get; private set; }
        public JsScope GlobalScope { get; private set; }

        public PermissionSet PermissionSet { get; set; }

        public bool AllowClr { get; set; }

        public CompiledBackend(Options options)
        {
            _options = options;
            _visitor = new Visitor(options, this);

            var global = new JsGlobal(this, options);

            Global = global;
            GlobalScope = new JsScope(global);
        }

        public object Run(Program program, bool unwrap)
        {
            if (program == null)
                throw new ArgumentNullException("program");

            _visitor.Reset();

            _visitor.Visit(program);

            _visitor.Close();

            var klass = _visitor.GetClassDeclaration();
            var compilationUnit = Syntax.CompilationUnit(
                usings: new[]
                {
                    Syntax.UsingDirective("System"),
                    Syntax.UsingDirective("System.Collections.Generic"),
                    Syntax.UsingDirective("System.Text"),
                    Syntax.UsingDirective("Jint"),
                    Syntax.UsingDirective("Jint.Native"),
                    Syntax.UsingDirective("Jint.Backend.Compiled")
                },
                members: new[]
                {
                    klass
                }
            );

            string code;

            using (var stringWriter = new StringWriter())
            {
                var writer = new SyntaxWriter(stringWriter);
                var printer = new SyntaxPrinter(writer);

                printer.Visit(compilationUnit);

                code = stringWriter.GetStringBuilder().ToString();
            }

            string fileName = Path.Combine(Path.GetTempPath(), "Program.cs");

            File.WriteAllText(fileName, code);

#if DEBUG
            var lines = code.Split('\n').Select(p => p.TrimEnd()).ToArray();
            int digits = (int)Math.Log10(lines.Length) + 1;
            var sb = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                int thisDigits = (int)Math.Log10(i + 1) + 1;
                sb.Append(new string(' ', digits - thisDigits));
                sb.Append(i + 1);
                sb.Append(": ");
                sb.AppendLine(lines[i]);
            }

            Console.WriteLine(sb.ToString());
#endif

            var codeProvider = new CSharpCodeProvider();
#pragma warning disable 612,618
            var compiler = codeProvider.CreateCompiler();
#pragma warning restore 612,618

            var parameters = new CompilerParameters
            {
                GenerateInMemory = true,
                ReferencedAssemblies =
                {
                    typeof(object).Assembly.Location,
                    GetType().Assembly.Location
                }
            };

#if DEBUG
            parameters.IncludeDebugInformation = true;
#endif

            var compilerResult = compiler.CompileAssemblyFromFile(parameters, fileName);

            foreach (var error in compilerResult.Errors)
            {
                Console.Error.WriteLine(error.ToString());
            }

            if (compilerResult.Errors.HasErrors)
                throw new InvalidOperationException("Could not compile program");

            _program = (JintProgram)Activator.CreateInstance(
                compilerResult.CompiledAssembly.GetType("Program"),
                new object[] { this, _options }
            );

            JsInstance result;

            try
            {
#if DEBUG
                var stopwatch = Stopwatch.StartNew();
#endif

                result = _program.Main();

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

                //if (_visitor.CurrentStatement.Source != null)
                //{
                //    source =
                //        Environment.NewLine +
                //        _visitor.CurrentStatement.Source + Environment.NewLine +
                //        _visitor.CurrentStatement.Source.Code;
                //}

                throw new JintException(message + source + stackTrace, e);
            }
            catch (Exception e)
            {
                var stackTrace = new StringBuilder();
                string source = String.Empty;

                //if (_visitor.CurrentStatement != null && _visitor.CurrentStatement.Source != null)
                //{
                //    source =
                //        Environment.NewLine +
                //        _visitor.CurrentStatement.Source + Environment.NewLine +
                //        _visitor.CurrentStatement.Source.Code;
                //}

                throw new JintException(e.Message + source + stackTrace, e);
            }

            return
                result == null
                ? null
                : unwrap
                    ? Global.Marshaller.MarshalJsValue<object>(result)
                    : result;
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

        public JsInstance ExecuteFunction(JsFunction function, JsDictionaryObject that, JsInstance[] parameters, Type[] genericParameters)
        {
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
            if ((_options & Options.Strict) != 0)
                functionScope.DefineOwnProperty(JsScope.Arguments, args);
            else
                args.DefineOwnProperty(JsScope.Arguments, args);

            // set this variable
            if (that != null)
                functionScope.DefineOwnProperty(JsScope.This, that);
            else
                functionScope.DefineOwnProperty(JsScope.This, that = Global as JsObject);

            try
            {
                if (AllowClr)
                    PermissionSet.PermitOnly();

                var previousScope = _program.EnterScope(functionScope);

                try
                {
                    if (!AllowClr || (genericParameters != null && genericParameters.Length == 0))
                        genericParameters = null;

                    return function.Execute(_visitor, that, parameters, genericParameters);
                }
                finally
                {
                    _program.ExitScope(previousScope);
                }
            }
            finally
            {
                if (AllowClr)
                    CodeAccessPermission.RevertPermitOnly();
            }
        }

        public int Compare(JsFunction function, JsInstance x, JsInstance y)
        {
            throw new NotImplementedException();
        }

        public object MarshalJsFunctionHelper(JsFunction func, Type delegateType)
        {
            throw new NotImplementedException();
        }
    }
}
