using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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

namespace Jint
{
    public class JintEngine
    {
        private readonly JintRuntime _runtime;
        private readonly ITypeResolver _typeResolver = CachedTypeResolver.Default;

        public bool IsClrAllowed { get; private set; }
        public PermissionSet PermissionSet { get; private set; }
        internal TypeSystem TypeSystem { get; private set; }

        public JsGlobal Global
        {
            get { return _runtime.Global; }
        }

        public JintEngine()
        {
            PermissionSet = new PermissionSet(PermissionState.None);
            TypeSystem = new TypeSystem();

            _runtime = new JintRuntime(this);
        }

        public JintEngine AllowClr()
        {
            return SetAllowClr(true);
        }

        public JintEngine SetAllowClr(bool allowed)
        {
            IsClrAllowed = true;
            return this;
        }

        public JintEngine SetParameter(string name, object value)
        {
            if (value == null)
            {
                value = JsNull.Instance;
            }
            else
            {
                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.Byte:
                    case TypeCode.Decimal:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.SByte:
                    case TypeCode.Single:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        value = Convert.ToDouble(value);
                        break;

                    case TypeCode.Double:
                    case TypeCode.Boolean:
                    case TypeCode.String:
                        break;

                    case TypeCode.Char:
                        value = new string((char)value, 1);
                        break;

                    case TypeCode.DateTime:
                        value = Global.CreateDate((DateTime)value);
                        break;

                    default:
                        if (!(
                            value is JsObject ||
                            value is JsNull ||
                            value is JsUndefined
                        ))
                            value = Global.WrapClr(value);
                        break;
                }
            }

            Global.GlobalScope.SetProperty(name, value);
            return this;
        }

        public JintEngine AddPermission(IPermission perm)
        {
            PermissionSet.AddPermission(perm);
            return this;
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

        public object ExecuteFile(string fileName)
        {
            return ExecuteFile(fileName, true);
        }

        public object ExecuteFile(string fileName, bool unwrap)
        {
            using (var reader = File.OpenText(fileName))
            {
                return Execute(reader.ReadToEnd(), fileName, unwrap);
            }
        }

        public object Execute(string script)
        {
            return Execute(script, null);
        }

        public object Execute(string script, string fileName)
        {
            return Execute(script, fileName, true);
        }

        public object Execute(string script, bool unwrap)
        {
            return Execute(script, null, unwrap);
        }

        public object Execute(string script, string fileName, bool unwrap)
        {
            if (script == null)
                throw new ArgumentNullException("script");

            ProgramSyntax program;

            try
            {
                program = Compile(script);
            }
            catch (Exception e)
            {
                throw new JsException(JsErrorType.SyntaxError, e.Message);
            }

            if (program == null)
                return unwrap ? null : JsNull.Instance;

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
                newBody = new BodySyntax(BodyType.Function, SyntaxNode.EmptyList, new VariableCollection(), false);
            }

            var function = new FunctionSyntax(null, newParameters, newBody, null, null);

            function.Accept(new VariableMarkerPhase(this));

            var scriptBuilder = TypeSystem.CreateScriptBuilder(null);
            var bindingVisitor = new BindingVisitor(scriptBuilder);

            var boundFunction = bindingVisitor.DeclareFunction(function);

            boundFunction = SquelchPhase.Perform(boundFunction);
            DefiniteAssignmentPhase.Perform(boundFunction);
            TypeMarkerPhase.Perform(boundFunction);

            return _runtime.CreateFunction(
                function.Name,
                (JsFunction)Delegate.CreateDelegate(
                    typeof(JsFunction),
                    new CodeGenerator(this, scriptBuilder).BuildFunction(boundFunction, sourceCode)
                ),
                function.Parameters.ToArray()
            );
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
                program.Accept(new VariableMarkerPhase(this));

                var scriptBuilder = TypeSystem.CreateScriptBuilder(fileName);
                var bindingVisitor = new BindingVisitor(scriptBuilder);

                program.Accept(bindingVisitor);

                var boundProgram = bindingVisitor.Program;
                var resultExpressions = DefiniteAssignmentPhase.Perform(boundProgram);
                boundProgram = ResultRewriterPhase.Perform(boundProgram, resultExpressions);
                TypeMarkerPhase.Perform(boundProgram);

                boundProgram = SquelchPhase.Perform(boundProgram);

                PrintBound(boundProgram);

                EnsureGlobalsDeclared(boundProgram);

                var method = new CodeGenerator(this, scriptBuilder).BuildMainMethod(boundProgram);

                result = method(_runtime);
            }

            if (result == null)
                return null;
            if (unwrap)
                return Global.Marshaller.MarshalJsValue<object>(result);

            return result;
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
            if (String.IsNullOrEmpty(source))
                return null;

            var lexer = new ES3Lexer(new ANTLRStringStream(source));
            var parser = new ES3Parser(new CommonTokenStream(lexer), source);

            var block = parser.ExecuteBlockStatements();

            if (parser.Errors != null && parser.Errors.Count > 0)
                throw new JintException(String.Join(Environment.NewLine, parser.Errors.ToArray()));

            return block;
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

        internal object ResolveUndefined(string typeFullName, Type[] generics)
        {
            if (!IsClrAllowed)
                throw new JsException(JsErrorType.ReferenceError);

            if (!String.IsNullOrEmpty(typeFullName))
            {
                if (!IsClrAllowed)
                    throw new SecurityException("Use of Clr is not allowed");

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

        [Conditional("DEBUG")]
        private void PrintBound(BoundProgram program)
        {
            var functions = FunctionGatherer.Gather(program.Body);

            var bodies = new List<BoundBody> { program.Body };
            bodies.AddRange(functions.Select(p => p.Body));

            using (var stream = File.Create("Bound Dump.txt"))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
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
    }
}
