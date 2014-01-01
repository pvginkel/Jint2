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
using Jint.Support;
using PropertyAttributes = Jint.Native.PropertyAttributes;

namespace Jint
{
    public partial class JintEngine
    {
        private readonly JintRuntime _runtime;
        private readonly ITypeResolver _typeResolver = CachedTypeResolver.Default;
        private readonly Dictionary<FunctionHash, JsFunction> _cachedFunctions = new Dictionary<FunctionHash, JsFunction>();
        private readonly Dictionary<ScriptHash, JsMain> _cachedScripts = new Dictionary<ScriptHash, JsMain>();

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

            var key = new ScriptHash(fileName, script);
            JsMain @delegate;

            if (!_cachedScripts.TryGetValue(key, out @delegate))
            {
                var compilationResult = CompileProgram(script, fileName);

                if (compilationResult.Main != null)
                {
                    @delegate = compilationResult.Main;
                    _cachedScripts.Add(key, @delegate);
                }
                else
                {
                    var literalResult = compilationResult.LiteralResult;

                    Debug.Assert(literalResult != null);

                    if (unwrap)
                        literalResult = Global.Marshaller.MarshalJsValue<object>(literalResult);

                    return literalResult;
                }
            }

            // Don't wrap exceptions while debugging to make stack traces
            // easier to read.

            object result;

#if !DEBUG
            try
            {
#endif

                result = @delegate(_runtime);
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

            if (result == null)
                return null;
            if (unwrap)
                return Global.Marshaller.MarshalJsValue<object>(result);

            return result;
        }

        private CompilationResult CompileProgram(string script, string fileName)
        {
            ProgramSyntax program;

            try
            {
                program = ParseProgram(script);

                if (program == null)
                    return new CompilationResult(JsNull.Instance, null);
            }
            catch (Exception e)
            {
                throw new JsException(JsErrorType.SyntaxError, e.Message);
            }

            program.Accept(new VariableMarkerPhase());

            var scriptBuilder = TypeSystem.CreateScriptBuilder(fileName);
            var bindingVisitor = new BindingVisitor(scriptBuilder);

            program.Accept(bindingVisitor);

            var boundProgram = bindingVisitor.Program;

            var interpreter = new JsonInterpreter(Global);
            if (boundProgram.Body.Accept(interpreter))
                return new CompilationResult(interpreter.Result, null);

            var resultExpressions = DefiniteAssignmentPhase.Perform(boundProgram);
            boundProgram = ResultRewriterPhase.Perform(boundProgram, resultExpressions);
            TypeMarkerPhase.Perform(boundProgram);

            boundProgram = SquelchPhase.Perform(boundProgram);

            PrintBound(boundProgram);

            EnsureGlobalsDeclared(boundProgram);

            var main = new CodeGenerator(Global, scriptBuilder).BuildMainMethod(boundProgram);

            return new CompilationResult(null, main);
        }

        private JsFunction CompileFunction(string sourceCode, IEnumerable<string> parameters)
        {
            BodySyntax newBody;

            if (sourceCode == String.Empty)
                newBody = new BodySyntax(BodyType.Function, SyntaxNode.EmptyList, new VariableCollection(), false);
            else
                newBody = ParseBlockStatement(sourceCode);

            var function = new FunctionSyntax(null, parameters, newBody, null, null);

            function.Accept(new VariableMarkerPhase());

            var scriptBuilder = TypeSystem.CreateScriptBuilder(null);
            var bindingVisitor = new BindingVisitor(scriptBuilder);

            var boundFunction = bindingVisitor.DeclareFunction(function);

            DefiniteAssignmentPhase.Perform(boundFunction);
            TypeMarkerPhase.Perform(boundFunction);

            boundFunction = SquelchPhase.Perform(boundFunction);

            return (JsFunction)Delegate.CreateDelegate(
                typeof(JsFunction),
                new CodeGenerator(Global, scriptBuilder).BuildFunction(boundFunction, sourceCode)
            );
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

            var parametersArray = newParameters.ToArray();

            string sourceCode =
                parameters.Length >= 1
                ? JsValue.ToString(parameters[parameters.Length - 1])
                : String.Empty;

            var key = new FunctionHash(parametersArray, sourceCode);
            JsFunction @delegate;

            if (!_cachedFunctions.TryGetValue(key, out @delegate))
            {
                @delegate = CompileFunction(sourceCode, parametersArray);

                _cachedFunctions.Add(key, @delegate);
            }

            return _runtime.CreateFunction(null, @delegate, parametersArray);
        }

        internal static ProgramSyntax ParseProgram(string source)
        {
            if (String.IsNullOrEmpty(source))
                return null;

            var lexer = new EcmaScriptLexer(new ANTLRStringStream(source));
            var parser = new EcmaScriptParser(new CommonTokenStream(lexer), source);

            var program = parser.Execute();

            if (parser.Errors != null && parser.Errors.Count > 0)
                throw new JintException(String.Join(Environment.NewLine, parser.Errors.ToArray()));

            return program;
        }

        private static BodySyntax ParseBlockStatement(string source)
        {
            if (String.IsNullOrEmpty(source))
                return null;

            var lexer = new EcmaScriptLexer(new ANTLRStringStream(source));
            var parser = new EcmaScriptParser(new CommonTokenStream(lexer), source);

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

        private class CompilationResult
        {
            public object LiteralResult { get; private set; }
            public JsMain Main { get; private set; }

            public CompilationResult(object literalResult, JsMain main)
            {
                LiteralResult = literalResult;
                Main = main;
            }
        }
    }
}
