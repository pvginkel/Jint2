using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using Jint.ExpressionExtensions;
using Jint.Expressions;
using Jint.Native;
using ValueType = Jint.Expressions.ValueType;

namespace Jint.Compiler
{
    internal partial class ExpressionVisitor : ISyntaxVisitor<Expression>
    {
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly MethodInfo _defineAccessorProperty = typeof(JsObject).GetMethod("DefineAccessorProperty");
        private static readonly MethodInfo _createArguments = typeof(JintRuntime).GetMethod("CreateArguments", InstanceFlags);
        private static readonly Dictionary<int, MethodInfo> _operationCache = BuildOperationCache();
        private const string RuntimeParameterName = "<>runtime";

        private static int _lastTypeId;
        private readonly Dictionary<SyntaxNode, string> _labels = new Dictionary<SyntaxNode, string>();

        private int _lastMethodId;
        private readonly HashSet<string> _compiledMethodNames = new HashSet<string>();
        private Scope _scope;
        private readonly JsGlobal _global;

        public ExpressionVisitor(JsGlobal global)
        {
            if (global == null)
                throw new ArgumentNullException("global");

            _global = global;
        }

        public Func<JintRuntime, JsBox> BuildMainMethod(LambdaExpression lambda)
        {
            if (lambda == null)
                throw new ArgumentNullException("lambda");

            var methodInfo = BuildMethod(
                "Main",
                typeof(JsBox),
                new[] { typeof(JintRuntime) },
                lambda
            );

            var result = (Func<JintRuntime, JsBox>)Delegate.CreateDelegate(
                typeof(Func<JintRuntime, JsBox>),
                methodInfo
            );

            // We've build a complete script. Dump the assembly (with the right
            // constants defined) so the generated assembly can be inspected.

            DynamicAssemblyManager.FlushAssembly();

            return result;
        }

        private MethodInfo BuildMethod(string name, Type returnType, Type[] parameterTypes, LambdaExpression lambda)
        {
            int typeId = Interlocked.Increment(ref _lastTypeId);

            var typeBuilder = DynamicAssemblyManager.ModuleBuilder.DefineType(
                "CompiledExpression" + typeId.ToString(CultureInfo.InvariantCulture),
                 TypeAttributes.Public
            );

            var method = typeBuilder.DefineMethod(
                name,
                MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig,
                returnType,
                parameterTypes
            );

            lambda.CompileToMethod(method);

            return typeBuilder.CreateType().GetMethod(name);
        }

        private static int GetOperationMethodKey(SyntaxExpressionType operation, ValueType a)
        {
            return GetOperationMethodKey(operation, a, ValueType.Unset);
        }

        private static int GetOperationMethodKey(SyntaxExpressionType operation, ValueType a, ValueType b)
        {
            return GetOperationMethodKey(operation, a, b, ValueType.Unset);
        }

        private static int GetOperationMethodKey(SyntaxExpressionType operation, ValueType a, ValueType b, ValueType c)
        {
            return (int)operation << 24 | (int)a << 16 | (int)b << 8 | (int)c;
        }

        private static Dictionary<int, MethodInfo> BuildOperationCache()
        {
            var result = new Dictionary<int, MethodInfo>();

            const string prefix = "Operation_";

            foreach (var method in typeof(JintRuntime).GetMethods())
            {
                if (method.Name.StartsWith(prefix))
                {
                    var operation = (SyntaxExpressionType)Enum.Parse(typeof(SyntaxExpressionType), method.Name.Substring(prefix.Length));

                    var parameters = method.GetParameters();

                    var a = SyntaxUtil.GetValueType(parameters[0].ParameterType);
                    var b =
                        parameters.Length < 2
                        ? ValueType.Unset
                        : SyntaxUtil.GetValueType(parameters[1].ParameterType);
                    var c =
                        parameters.Length < 3
                        ? ValueType.Unset
                        : SyntaxUtil.GetValueType(parameters[2].ParameterType);

                    result[GetOperationMethodKey(operation, a, b, c)] = method;
                }
            }

            return result;
        }

        private static MethodInfo FindOperationMethod(SyntaxExpressionType operation, ValueType operand)
        {
            return FindOperationMethod(operation, operand, ValueType.Unset);
        }

        private static MethodInfo FindOperationMethod(SyntaxExpressionType operation, ValueType left, ValueType right)
        {
            return FindOperationMethod(operation, left, right, ValueType.Unset);
        }

        private static MethodInfo FindOperationMethod(SyntaxExpressionType operation, ValueType a, ValueType b, ValueType c)
        {
            MethodInfo result;
            _operationCache.TryGetValue(GetOperationMethodKey(operation, a, b, c), out result);
            return result;
        }

        public Expression VisitProgram(ProgramSyntax syntax)
        {
            var statements = new List<Expression>();
            var locals = new List<ParameterExpression>();
            
            var that = Expression.Parameter(typeof(JsBox), "this");

            ParameterExpression closureLocal = null;

            if (syntax.Closure != null)
            {
                closureLocal = Expression.Parameter(
                    syntax.Closure.Type,
                    "closure"
                );
                locals.Add(closureLocal);
            }

            _scope = new Scope(
                this,
                that,
                syntax.Closure,
                null,
                closureLocal,
                null,
                statements,
                null
            );

            locals.Add(that);

            if (closureLocal != null)
            {
                // We don't add our closureLocal to locals here because
                // later we add everything from _scope.ClosureLocals.
                _scope.ClosureLocals.Add(syntax.Closure, closureLocal);

                statements.Add(Expression.Assign(
                    closureLocal,
                    Expression.New(
                        closureLocal.Type.GetConstructors()[0]
                    )
                ));
            }

            statements.Add(Expression.Assign(
                that,
                EnsureJs(Expression.Property(
                    _scope.Runtime,
                    "GlobalScope"
                ))
            ));

            // Build the body and return label.

            var body = ProcessFunctionBody(syntax, _scope.Runtime);

            if (body.Type == typeof(void))
            {
                statements.Add(body);
                statements.Add(Expression.Label(
                    _scope.Return,
                    Expression.Field(null, typeof(JsBox).GetField("Undefined"))
                ));
            }
            else
            {
                statements.Add(Expression.Label(
                    _scope.Return,
                    EnsureJs(body)
                ));
            }

            return Expression.Lambda<Func<JintRuntime, JsBox>>(
                Expression.Block(
                    locals,
                    statements
                ),
                new[] { _scope.Runtime }
            );
        }

        private BlockExpression ProcessFunctionBody(BlockSyntax syntax, ParameterExpression runtimeParameter)
        {
            // Declare the locals.

            var parameters = new List<ParameterExpression>();

            foreach (var variable in syntax.DeclaredVariables)
            {
                if (variable.Type == VariableType.Local)
                {
                    var parameter = Expression.Parameter(
                        typeof(JsBox),
                        variable.Name
                    );

                    parameters.Add(parameter);
                    _scope.Variables.Add(variable, parameter);
                }
            }

            // Accept all statements.

            var statements = new List<Expression>();

            foreach (var item in syntax.Statements)
            {
                statements.Add(item.Accept(this));
            }

            return Expression.Block(
                parameters,
                statements
            );
        }

        public Expression VisitAssignment(AssignmentSyntax syntax)
        {
            if (syntax.Operation == AssignmentOperator.Assign)
                return BuildSet(syntax.Left, syntax.Right.Accept(this));

            return BuildSet(
                syntax.Left,
                new BinaryExpressionSyntax(
                    AssignmentSyntax.GetSyntaxType(syntax.Operation),
                    syntax.Left,
                    syntax.Right
                ).Accept(this)
            );
        }

        public Expression VisitBlock(BlockSyntax syntax)
        {
            if (syntax.Statements.Count == 0)
                return Expression.Empty();

            return Expression.Block(
                syntax.Statements.Select(p => p.Accept(this))
            );
        }

        public Expression VisitBreak(BreakSyntax syntax)
        {
            return Expression.Goto(FindLabelTarget(_scope.BreakTargets, syntax.Target));
        }

        public Expression VisitContinue(ContinueSyntax syntax)
        {
            return Expression.Goto(FindLabelTarget(_scope.ContinueTargets, syntax.Target));
        }

        private LabelTarget FindLabelTarget(Stack<LabelTarget> targets, string label)
        {
            if (targets.Count == 0)
                throw new InvalidOperationException("There is not label");

            if (label != null)
            {
                var target = targets.LastOrDefault(p => p.Name == label);
                if (target == null)
                    throw new InvalidOperationException("Cannot find break target " + label);

                return target;
            }

            return targets.Peek();
        }

        public Expression VisitDoWhile(DoWhileSyntax syntax)
        {
            // Create the break and continue targets and push them onto the stack.

            var breakTarget = Expression.Label(GetLabel(syntax) ?? "<>break");
            _scope.BreakTargets.Push(breakTarget);
            var continueTarget = Expression.Label(GetLabel(syntax) ?? "<>continue");
            _scope.ContinueTargets.Push(continueTarget);

            var result = Expression.Loop(
                Expression.Block(
                    typeof(void),
                    // Execute the body.
                    syntax.Body.Accept(this),
                    Expression.Label(continueTarget),
                    // At the end of every iteration, perform the test.
                    Expression.IfThenElse(
                        EnsureBoolean(syntax.Test.Accept(this)),
                        Expression.Empty(),
                        Expression.Goto(breakTarget)
                    )
                ),
                breakTarget
            );

            // Pop the break and continue targets to make the previous ones
            // available.

            _scope.BreakTargets.Pop();
            _scope.ContinueTargets.Pop();

            return result;
        }

        private string GetLabel(SyntaxNode syntax)
        {
            string label;
            _labels.TryGetValue(syntax, out label);
            return label;
        }

        public Expression VisitEmpty(EmptySyntax syntax)
        {
            return Expression.Empty();
        }

        public Expression VisitExpressionStatement(ExpressionStatementSyntax syntax)
        {
            return syntax.Expression.Accept(this);
        }

        public Expression VisitForEachIn(ForEachInSyntax syntax)
        {
            // Create break and continue labels and push them onto the stack.

            var breakTarget = Expression.Label(GetLabel(syntax) ?? "<>break");
            _scope.BreakTargets.Push(breakTarget);
            var continueTarget = Expression.Label(GetLabel(syntax) ?? "<>continue");
            _scope.ContinueTargets.Push(continueTarget);

            // Temporary variable to hold a reference to the current key.

            var keyLocal = Expression.Parameter(typeof(JsBox), "key");

            var result = ExpressionEx.ForEach(
                keyLocal,
                typeof(JsBox),
                // Call JintRuntime.GetForEachKeys to get the keys to enumerate over.
                Expression.Call(
                    _scope.Runtime,
                    typeof(JintRuntime).GetMethod("GetForEachKeys"),
                    EnsureJs(syntax.Expression.Accept(this))
                ),
                Expression.Block(
                    typeof(void),
                    // Copy the current key to the provided target.
                    BuildSet(syntax.Initialization, keyLocal),
                    syntax.Body.Accept(this)
                ),
                breakTarget,
                continueTarget
            );

            // Pop the break and continue labels to make the previous ones available.

            _scope.BreakTargets.Pop();
            _scope.ContinueTargets.Pop();

            return result;
        }

        public Expression VisitFor(ForSyntax syntax)
        {
            var statements = new List<Expression>();

            // Push the break and continue targets onto the stack.

            var breakTarget = Expression.Label(GetLabel(syntax) ?? "<>break");
            _scope.BreakTargets.Push(breakTarget);
            var continueTarget = Expression.Label(GetLabel(syntax) ?? "<>continue");
            _scope.ContinueTargets.Push(continueTarget);

            // At the start of our block, we perform any initialization.

            if (syntax.Initialization != null)
                statements.Add(syntax.Initialization.Accept(this));

            var test =
                syntax.Test != null
                ? syntax.Test.Accept(this)
                : null;

            var increment =
                syntax.Increment != null
                ? syntax.Increment.Accept(this)
                : null;

            var body =
                syntax.Body != null
                ? syntax.Body.Accept(this)
                : null;

            var loopStatements = new List<Expression>();

            // If we have a test, we perform the test at the start of every
            // iteration. The test is itself an expression which we here
            // convert to a bool to be able to test it. The else branch
            // is used to invert the result.

            if (test != null)
            {
                loopStatements.Add(Expression.IfThenElse(
                    EnsureBoolean(test),
                    Expression.Empty(),
                    Expression.Goto(breakTarget)
                ));
            }

            // Add the body.

            if (body != null)
                loopStatements.Add(body);

            // Increment is done at the end.

            loopStatements.Add(Expression.Label(continueTarget));

            if (increment != null)
                loopStatements.Add(increment);

            statements.Add(Expression.Loop(
                Expression.Block(
                    loopStatements
                ),
                breakTarget
            ));

            // Remove the break and continue targets to make the previous ones
            // visible.

            _scope.BreakTargets.Pop();
            _scope.ContinueTargets.Pop();

            return Expression.Block(
                typeof(void),
                statements
            );
        }

        public Expression VisitFunctionDeclaration(FunctionDeclarationSyntax syntax)
        {
            return _scope.BuildSet(
                syntax.Target,
                CreateFunctionSyntax(syntax, DeclareFunction(syntax))
            );
        }

        public Expression VisitIf(IfSyntax syntax)
        {
            return Expression.IfThenElse(
                EnsureBoolean(syntax.Test.Accept(this)),
                syntax.Then != null ? syntax.Then.Accept(this) : Expression.Empty(),
                syntax.Else != null ? syntax.Else.Accept(this) : Expression.Empty()
            );
        }

        public Expression VisitReturn(ReturnSyntax syntax)
        {
            return Expression.Goto(
                _scope.Return,
                syntax.Expression != null
                ? EnsureJs(syntax.Expression.Accept(this))
                : Expression.Default(typeof(JsBox))
            );
        }

        public Expression VisitSwitch(SwitchSyntax syntax)
        {
            // Create the label that jumps to the end of the switch.
            var after = Expression.Label(GetLabel(syntax) ?? "<>after");
            _scope.BreakTargets.Push(after);

            var statements = new List<Expression>();

            var expression = syntax.Expression.Accept(this);

            var expressionLocal = Expression.Parameter(
                SyntaxUtil.GetTargetType(expression.Type),
                "expression"
            );

            statements.Add(Expression.Assign(
                expressionLocal,
                expression
            ));

            var bodies = new List<Tuple<LabelTarget, Expression>>();

            foreach (var caseClause in syntax.Cases)
            {
                var target = Expression.Label("target" + bodies.Count);

                statements.Add(Expression.IfThen(
                    BuildOperationCall(
                        SyntaxExpressionType.Equal,
                        expressionLocal,
                        caseClause.Expression.Accept(this)
                    ),
                    Expression.Goto(target)
                ));

                bodies.Add(Tuple.Create(target, caseClause.Body.Accept(this)));
            }

            if (syntax.Default != null)
            {
                var target = Expression.Label("default");

                statements.Add(Expression.Goto(target));

                bodies.Add(Tuple.Create(target, syntax.Default.Accept(this)));
            }

            foreach (var body in bodies)
            {
                statements.Add(Expression.Label(body.Item1));
                statements.Add(body.Item2);
            }

            statements.Add(Expression.Label(after));

            // Pop the break target to make the previous one available.
            _scope.BreakTargets.Pop();

            return Expression.Block(
                typeof(void),
                new[] { expressionLocal },
                statements
            );
        }

        public Expression VisitWith(WithSyntax syntax)
        {
            return Expression.Block(
                _scope.BuildSet(syntax.Target, syntax.Expression.Accept(this)),
                syntax.Body.Accept(this)
            );
        }

        public Expression VisitThrow(ThrowSyntax syntax)
        {
            return Expression.Throw(
                Expression.New(
                    typeof(JsException).GetConstructor(new[] { typeof(JsBox) }),
                    EnsureJs(syntax.Expression.Accept(this))
                )
            );
        }

        public Expression VisitTry(TrySyntax syntax)
        {
            CatchBlock[] catches = null;

            var catchBlock = syntax.Catch;
            if (catchBlock != null)
            {
                var exception = Expression.Parameter(typeof(Exception), "exception");

                var catchStatements = new List<Expression>();

                if (catchBlock.Identifier != null)
                {
                    catchStatements.Add(_scope.BuildSet(
                        catchBlock.Target,
                        Expression.Call(
                            _scope.Runtime,
                            typeof(JintRuntime).GetMethod("WrapException"),
                            exception
                        )
                    ));
                }

                catchStatements.Add(catchBlock.Body.Accept(this));

                catches = new[]
                {
                    Expression.MakeCatchBlock(
                        typeof(Exception),
                        exception,
                        Expression.Block(
                            typeof(void),
                            catchStatements
                        ),
                        null
                    )
                };
            }

            var body = syntax.Body.Accept(this);

            if (body.Type != typeof(void))
                body = Expression.Block(typeof(void), body);

            Expression finallyBody = null;

            if (syntax.Finally != null)
            {
                finallyBody = syntax.Finally.Body.Accept(this);
                if (finallyBody.Type != typeof(void))
                    finallyBody = Expression.Block(typeof(void), finallyBody);
            }

            return Expression.TryCatchFinally(
                body,
                finallyBody,
                catches
            );
        }

        public Expression VisitVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            if (syntax.Expression == null)
                return Expression.Empty();

            return _scope.BuildSet(
                syntax.Target,
                syntax.Expression.Accept(this)
            );
        }

        public Expression VisitWhile(WhileSyntax syntax)
        {
            // Create the break and continue targets and push them onto the stack.

            var breakTarget = Expression.Label(GetLabel(syntax) ?? "<>break");
            _scope.BreakTargets.Push(breakTarget);
            var continueTarget = Expression.Label(GetLabel(syntax) ?? "<>continue");
            _scope.ContinueTargets.Push(continueTarget);

            var result = Expression.Loop(
                Expression.Block(
                    typeof(void),
                    // At the beginning of every iteration, perform the test.
                    Expression.IfThenElse(
                        EnsureBoolean(syntax.Test.Accept(this)),
                        Expression.Empty(),
                        Expression.Goto(breakTarget)
                    ),
                    // Execute the body.
                    syntax.Body.Accept(this)
                ),
                breakTarget,
                continueTarget
            );

            // Pop the break and continue targets to make the previous ones
            // available.

            _scope.BreakTargets.Pop();
            _scope.ContinueTargets.Pop();

            return result;
        }

        public Expression VisitArrayDeclaration(ArrayDeclarationSyntax syntax)
        {
            var statements = new List<Expression>();

            var array = Expression.Parameter(typeof(JsObject), "array");

            statements.Add(Expression.Assign(
                array,
                Expression.Call(
                    Expression.Property(_scope.Runtime, "Global"),
                    typeof(JsGlobal).GetMethod("CreateArray")
                )
            ));

            for (int i = 0; i < syntax.Parameters.Count; i++)
            {
                statements.Add(BuildSetIndex(
                    array,
                    EnsureJs(Expression.Constant(i.ToString(CultureInfo.InvariantCulture))),
                    syntax.Parameters[i].Accept(this)
                ));
            }

            statements.Add(array);

            return Expression.Block(
                typeof(JsObject),
                new[] { array },
                statements
            );
        }

        public Expression VisitCommaOperator(CommaOperatorSyntax syntax)
        {
            return Expression.Block(
                syntax.Expressions.Select(p => p.Accept(this))
            );
        }

        public Expression VisitFunction(FunctionSyntax syntax)
        {
            var compiledFunction = CreateFunctionSyntax(syntax, DeclareFunction(syntax));

            if (syntax.Target != null)
                return _scope.BuildSet(syntax.Target, compiledFunction);
            else
                return compiledFunction;
        }

        private Expression CreateFunctionSyntax(IFunctionDeclaration function, MethodInfo compiledFunction)
        {
            return Expression.Call(
                _scope.Runtime,
                typeof(JintRuntime).GetMethod("CreateFunction"),
                Expression.Constant(function.Name, typeof(string)),
                Expression.Constant(compiledFunction, typeof(MethodInfo)),
                _scope.ClosureLocal != null ? (Expression)_scope.ClosureLocal : Expression.Constant(null),
                MakeArrayInit(function.Parameters.Select(Expression.Constant), typeof(string), true)
            );
        }

        public MethodInfo DeclareFunction(IFunctionDeclaration function)
        {
            var body = function.Body;

            var thisParameter = Expression.Parameter(
                typeof(JsBox),
                "this"
            );

            var functionParameter = Expression.Parameter(
                typeof(JsObject),
                "function"
            );
            
            ParameterExpression closureLocal = null;
            var scopedClosure = FindScopedClosure(body, _scope);

            if (scopedClosure != null)
            {
                closureLocal = Expression.Parameter(
                    scopedClosure.Type,
                    "closure"
                );
            }

            var argumentsParameter = Expression.Parameter(
                typeof(JsBox[]),
                "argumentsParameter"
            );

            var statements = new List<Expression>();

#if TRACE_CALLSTACK
            var source = ((SyntaxNode)function).Source;

            statements.Add(Expression.Call(
                typeof(Trace).GetMethod("WriteLine", new[] { typeof(string) }),
                Expression.Constant(String.Format("Entering {0}:{1}", source.Start.Line, source.Start.Char))
            ));
#endif

            _scope = new Scope(
                this,
                thisParameter,
                scopedClosure,
                functionParameter,
                closureLocal,
                body.DeclaredVariables.Single(p => p.Type == Expressions.VariableType.Arguments),
                statements,
                _scope
            );

            var closureParameter = Expression.Parameter(
                typeof(object),
                "closureParameter"
            );

            var locals = new List<ParameterExpression>();

            var parameters = new List<ParameterExpression>
            {
                _scope.Runtime,
                thisParameter,
                functionParameter,
                closureParameter,
                argumentsParameter,
                Expression.Parameter(typeof(JsBox[]), "genericArguments")
            };

            // Initialize our closure.

            if (closureLocal != null)
            {
                // We don't add our closureLocal to locals here because
                // later we add everything from _scope.ClosureLocals.
                _scope.ClosureLocals.Add(scopedClosure, closureLocal);

                // Only instantiate the closure when its our closure (and not
                // just a copy of the parent closure).

                if (body.Closure != null)
                {
                    statements.Add(Expression.Assign(
                        closureLocal,
                        Expression.New(
                            closureLocal.Type.GetConstructors()[0]
                        )
                    ));

                    // If the closure contains a link to a parent closure,
                    // assign it here.

                    var parentField = scopedClosure.Type.GetField(Closure.ParentFieldName);
                    if (parentField != null)
                    {
                        statements.Add(Expression.Assign(
                            Expression.Field(closureLocal, parentField),
                            Expression.Convert(closureParameter, parentField.FieldType)
                        ));
                    }

                    // Initialize all fields of the closure.

                    foreach (var field in body.Closure.Type.GetFields())
                    {
                        if (field.FieldType == typeof(JsBox))
                        {
                            statements.Add(Expression.Assign(
                                Expression.Field(closureLocal, field),
                                Expression.Field(null, typeof(JsBox).GetField("Undefined"))
                            ));
                        }
                    }
                }
                else
                {
                    statements.Add(Expression.Assign(
                        closureLocal,
                        Expression.Convert(closureParameter, scopedClosure.Type)
                    ));
                }
            }

            // Build the locals.

            foreach (var declaredVariable in body.DeclaredVariables)
            {
                if (
                    (
                        declaredVariable.Type == VariableType.Local ||
                        declaredVariable.Type == VariableType.Arguments
                    ) &&
                    declaredVariable.ClosureField == null
                ) {
                    var local = Expression.Parameter(
                        declaredVariable.NativeType,
                        declaredVariable.Name
                    );
                    locals.Add(local);

                    if (local.Type == typeof(JsBox))
                    {
                        statements.Add(Expression.Assign(
                            local,
                            Expression.Field(null, typeof(JsBox).GetField("Undefined"))
                        ));
                    }

                    _scope.Variables.Add(declaredVariable, local);
                }
            }

            // Initialize the arguments array.

            statements.Add(_scope.BuildSet(
                _scope.ArgumentsVariable,
                Expression.Call(
                    _scope.Runtime,
                    _createArguments,
                    functionParameter,
                    argumentsParameter
                )
            ));

            // Build the body.

            var bodyExpression = body.Accept(this);

#if TRACE_CALLSTACK
            // Exit trace.

            bodyExpression = Expression.TryFinally(
                bodyExpression,
                Expression.Call(
                    typeof(Trace).GetMethod("WriteLine", new[] { typeof(string) }),
                    Expression.Constant(String.Format("Exiting {0}:{1}", source.Stop.Line, source.Stop.Char))
                )
            );

#endif

            statements.Add(bodyExpression);

            // Add the return label.

            statements.Add(Expression.Label(
                _scope.Return,
                Expression.Field(null, typeof(JsBox).GetField("Undefined"))
            ));

            // Add all gathered locals for the closures to the locals list.

            foreach (var local in _scope.ClosureLocals.Values)
            {
                locals.Add(local);
            }

            // Add the arguments if one was created.

            var lambda = Expression.Lambda<JsFunction>(
                Expression.Block(
                    typeof(JsBox),
                    locals,
                    statements
                ),
                parameters
            );

            _scope = _scope.Parent;

            JintEngine.PrintExpression(lambda);

            return BuildMethod(
                GetFunctionName(function),
                typeof(JsBox),
                new[] { typeof(JintRuntime), typeof(JsBox), typeof(JsObject), typeof(object), typeof(JsBox[]), typeof(JsBox[]) },
                lambda
            );
        }

        private string GetFunctionName(IFunctionDeclaration function)
        {
            string name;

            if (function.Name == null)
            {
                name = (++_lastMethodId).ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                name = function.Name;

                for (int i = 0;; i++)
                {
                    if (i > 0)
                        name = function.Name + "_" + i.ToString(CultureInfo.InvariantCulture);

                    if (!_compiledMethodNames.Contains(name))
                        break;
                }
            }

            _compiledMethodNames.Add(name);

            return name;
        }

        private Closure FindScopedClosure(BlockSyntax body, Scope scope)
        {
            if (body.Closure != null)
                return body.Closure;

            while (scope != null)
            {
                if (scope.Closure != null)
                    return scope.Closure;

                scope = scope.Parent;
            }

            return null;
        }

        public Expression VisitMethodCall(MethodCallSyntax syntax)
        {
            Expression getter;
            Expression target;
            var statements = new List<Expression>();
            var parameters = new List<ParameterExpression>();

            var memberSyntax = syntax.Expression as MemberSyntax;
            if (memberSyntax != null)
            {
                // We need to get a hold on the object we need to execute on.
                // This applies to an index and property. The target is stored in
                // a local and both the getter and the ExecuteFunction is this
                // local.

                var targetAssignment = EnsureJs(memberSyntax.Expression.Accept(this));

                if (targetAssignment is ParameterExpression)
                {
                    target = targetAssignment;
                }
                else
                {
                    target = Expression.Parameter(typeof(JsBox), "target");

                    statements.Add(Expression.Assign(target, targetAssignment));
                    parameters.Add((ParameterExpression)target);
                }

                if (memberSyntax.Type == SyntaxType.Property)
                    getter = BuildGetMember(target, ((PropertySyntax)memberSyntax).Name);
                else
                    getter = BuildGetIndex(target, BuildGet(((IndexerSyntax)memberSyntax).Index));
            }
            else
            {
                var identifierSyntax = syntax.Expression as IdentifierSyntax;

                if (
                    identifierSyntax != null &&
                    identifierSyntax.Target.Type == VariableType.WithScope
                )
                {
                    // With a with scope, the target depends on how the variable
                    // is resolved.

                    var withTarget = Expression.Parameter(typeof(JsBox), "target");
                    statements.Add(Expression.Assign(
                        withTarget,
                        Expression.Default(typeof(JsBox))
                    ));

                    var method = Expression.Parameter(
                        typeof(JsBox),
                        "method"
                    );

                    parameters.Add(method);

                    statements.Add(Expression.Assign(
                        method,
                        BuildGet(identifierSyntax, withTarget)
                    ));

                    getter = method;

                    parameters.Add(withTarget);
                    target = withTarget;
                }
                else
                {
                    // Else we execute the function against the global scope.

                    target = EnsureJs(Expression.Property(
                        _scope.Runtime,
                        "GlobalScope"
                    ));
                    getter = BuildGet(syntax.Expression);
                }
            }

            bool needWriteBack = false;

            Expression argumentsExpression;

            if (syntax.Arguments.Count > 0)
            {
                var expressions = new List<Expression>();

                var arguments = Expression.Parameter(
                    typeof(JsBox[]),
                    "arguments"
                );
                parameters.Add(arguments);

                foreach (var argument in syntax.Arguments)
                {
                    expressions.Add(EnsureJs(argument.Expression.Accept(this)));

                    if (argument.IsRef && argument.Expression.IsAssignable)
                        needWriteBack = true;
                }

                var argumentsInit = MakeArrayInit(expressions, typeof(JsBox), false);

                if (needWriteBack)
                {
                    statements.Add(Expression.Assign(
                        arguments,
                        argumentsInit
                    ));

                    argumentsExpression = arguments;
                }
                else
                {
                    argumentsExpression = argumentsInit;
                }
            }
            else
            {
                argumentsExpression = Expression.Field(
                    null,
                    typeof(JsBox).GetField("EmptyArray")
                );
            }

            var callExpression = Expression.Call(
                Expression.Convert(getter, typeof(JsObject)),
                typeof(JsObject).GetMethod("Execute"),
                _scope.Runtime,
                target,
                argumentsExpression,
                MakeArrayInit(syntax.Generics, typeof(JsBox), true)
            );

            if (needWriteBack)
            {
                var result = Expression.Parameter(
                    typeof(JsBox),
                    "result"
                );
                parameters.Add(result);

                statements.Add(Expression.Assign(
                    result,
                    callExpression
                ));

                // We need to read the arguments back for when the ExecuteFunction
                // has out parameters for native calls.

                for (int i = 0; i < syntax.Arguments.Count; i++)
                {
                    var arguments = (ParameterExpression)argumentsExpression;
                    var argument = syntax.Arguments[i];

                    if (argument.IsRef && argument.Expression.IsAssignable)
                    {
                        statements.Add(BuildSet(
                            argument.Expression,
                            Expression.ArrayAccess(
                                arguments,
                                Expression.Constant(i)
                            )
                        ));
                    }
                }

                statements.Add(result);
            }
            else
            {
                statements.Add(callExpression);
            }

            if (statements.Count == 1)
                return statements[0];

            return Expression.Block(
                typeof(JsBox),
                parameters,
                statements
            );
        }

        public Expression VisitIndexer(IndexerSyntax syntax)
        {
            return BuildGet(syntax);
        }

        public Expression VisitProperty(PropertySyntax syntax)
        {
            return BuildGet(syntax);
        }

        public Expression VisitIdentifier(IdentifierSyntax syntax)
        {
            return BuildGet(syntax);
        }

        public Expression VisitJsonExpression(JsonExpressionSyntax syntax)
        {
            var global = Expression.Parameter(typeof(JsGlobal), "global");
            var obj = Expression.Parameter(typeof(JsObject), "obj");

            var statements = new List<Expression>
            {
                Expression.Assign(
                    global,
                    Expression.Property(
                        _scope.Runtime,
                        typeof(JintRuntime).GetProperty("Global")
                    )
                ),
                Expression.Assign(
                    obj,
                    Expression.Call(
                        global,
                        typeof(JsGlobal).GetMethod("CreateObject", new Type[0])
                    )
                )
            };

            foreach (var property in syntax.Properties)
            {
                var dataProperty = property as JsonDataProperty;
                if (dataProperty != null)
                {
                    statements.Add(BuildSetMember(
                        obj,
                        dataProperty.Name,
                        dataProperty.Expression.Accept(this)
                    ));
                }
                else
                {
                    var accessorProperty = (JsonAccessorProperty)property;

                    statements.Add(Expression.Call(
                        obj,
                        _defineAccessorProperty,
                        new[]
                        {
                            Expression.Constant(_global.ResolveIdentifier(accessorProperty.Name)),
                            accessorProperty.GetExpression != null
                                ? accessorProperty.GetExpression.Accept(this)
                                : Expression.Default(typeof(JsObject)),
                            accessorProperty.SetExpression != null
                                ? accessorProperty.SetExpression.Accept(this)
                                : Expression.Default(typeof(JsObject))
                        }
                    ));
                }
            }

            statements.Add(obj);

            return Expression.Block(
                typeof(JsObject),
                new[] { obj, global },
                statements
            );
        }

        public Expression VisitNew(NewSyntax syntax)
        {
            var methodCall = syntax.Expression as MethodCallSyntax;

            IList<MethodArgument> arguments = null;
            IList<ExpressionSyntax> generics = null;
            var expression = syntax.Expression;

            if (methodCall != null)
            {
                arguments = methodCall.Arguments;
                generics = methodCall.Generics;
                expression = methodCall.Expression;
            }

            return Expression.Call(
                _scope.Runtime,
                typeof(JintRuntime).GetMethod("New"),
                expression.Accept(this),
                MakeArrayInit(
                    arguments == null ? null : arguments.Select(p => p.Expression),
                    typeof(JsBox),
                    false
                ),
                MakeArrayInit(generics, typeof(JsBox), true)
            );
        }

        private Expression MakeArrayInit(IEnumerable<SyntaxNode> initializers, Type elementType, bool nullWhenEmpty)
        {
            if (initializers == null)
                initializers = new SyntaxNode[0];

            return MakeArrayInit(initializers.Select(p => EnsureJs(p.Accept(this))), elementType, nullWhenEmpty);
        }

        public static Expression MakeArrayInit(IEnumerable<Expression> initializers, Type elementType, bool nullWhenEmpty)
        {
            var expressions = initializers.ToList();

            if (expressions.Count == 0)
            {
                if (nullWhenEmpty)
                    return Expression.Constant(null, elementType.MakeArrayType());

                if (elementType == typeof(JsBox[]))
                    return Expression.Field(null, typeof(JsBox).GetField("EmptyArray"));

                return Expression.NewArrayBounds(elementType, Expression.Constant(0));
            }

            return Expression.NewArrayInit(elementType, expressions);
        }

        public Expression VisitBinaryExpression(BinaryExpressionSyntax syntax)
        {
            switch (syntax.Operation)
            {
                case SyntaxExpressionType.And:
                case SyntaxExpressionType.Or:
                    var left = syntax.Left.Accept(this);
                    var leftType = SyntaxUtil.GetValueType(left.Type);
                    var right = syntax.Right.Accept(this);
                    var rightType = SyntaxUtil.GetValueType(right.Type);

                    ValueType targetType;
                    if (leftType == rightType)
                    {
                        targetType = leftType;
                    }
                    else
                    {
                        left = EnsureJs(left);
                        right = EnsureJs(right);
                        targetType = ValueType.Unknown;
                    }

                    var tmp = Expression.Parameter(targetType.ToType(), "tmp");

                    if (syntax.Operation == SyntaxExpressionType.And)
                    {
                        return Expression.Block(
                            tmp.Type,
                            new[] { tmp },
                            Expression.Assign(tmp, left),
                            Expression.Condition(
                                EnsureBoolean(tmp),
                                right,
                                tmp,
                                tmp.Type
                            )
                        );
                    }
                    else
                    {
                        return Expression.Block(
                            tmp.Type,
                            new[] { tmp },
                            Expression.Assign(tmp, left),
                            Expression.Condition(
                                EnsureBoolean(tmp),
                                tmp,
                                right,
                                tmp.Type
                            )
                        );
                    }

                default:
                    return BuildOperationCall(syntax.Operation, syntax.Left.Accept(this), syntax.Right.Accept(this));
            }
        }

        private Expression EnsureJs(Expression expression)
        {
            var type = SyntaxUtil.GetValueType(expression.Type);
            if (type == ValueType.Unknown)
                return expression;

            return new ConvertToJsExpression(_scope.Runtime, expression);
        }

        private Expression EnsureBoolean(Expression expression)
        {
            if (SyntaxUtil.GetValueType(expression.Type) == ValueType.Boolean)
                return expression;

            return new ConvertFromJsExpression(EnsureJs(expression), ValueType.Boolean);
        }

        private Expression EnsureString(Expression expression)
        {
            if (SyntaxUtil.GetValueType(expression.Type) == ValueType.String)
                return expression;

            return new ConvertFromJsExpression(EnsureJs(expression), ValueType.String);
        }

        private Expression EnsureNumber(Expression expression)
        {
            if (SyntaxUtil.GetValueType(expression.Type) == ValueType.Double)
                return expression;

            return new ConvertFromJsExpression(EnsureJs(expression), ValueType.Double);
        }

        private Expression EnsureObject(Expression expression)
        {
            if (SyntaxUtil.GetValueType(expression.Type) == ValueType.Object)
                return expression;

            return new ConvertFromJsExpression(EnsureJs(expression), ValueType.Object);
        }

        public Expression VisitTernary(TernarySyntax syntax)
        {
            var then = syntax.Then.Accept(this);
            var thenType = SyntaxUtil.GetValueType(then.Type);
            var @else = syntax.Else.Accept(this);
            var elseType = SyntaxUtil.GetValueType(@else.Type);

            Type targetType;
            if (thenType == elseType)
            {
                targetType = then.Type;
            }
            else
            {
                targetType = typeof(JsBox);
                then = EnsureJs(then);
                @else = EnsureJs(@else);
            }

            return Expression.Condition(
                EnsureBoolean(syntax.Test.Accept(this)),
                then,
                @else,
                targetType
            );
        }

        public Expression VisitUnaryExpression(UnaryExpressionSyntax syntax)
        {
            switch (syntax.Operation)
            {
                case SyntaxExpressionType.PostIncrementAssign:
                case SyntaxExpressionType.PostDecrementAssign:
                case SyntaxExpressionType.PreIncrementAssign:
                case SyntaxExpressionType.PreDecrementAssign:
                    bool isIncrement = syntax.Operation == SyntaxExpressionType.PreIncrementAssign || syntax.Operation == SyntaxExpressionType.PostIncrementAssign;
                    bool isPrefix = syntax.Operation == SyntaxExpressionType.PreDecrementAssign || syntax.Operation == SyntaxExpressionType.PreIncrementAssign;

                    var source = BuildGet(syntax.Operand);

                    var tmp = Expression.Parameter(
                        SyntaxUtil.GetTargetType(source.Type),
                        "tmp"
                    );

                    var calculationExpression = BuildOperationCall(
                        isIncrement ? SyntaxExpressionType.Add : SyntaxExpressionType.Subtract,
                        tmp,
                        Expression.Constant(1d)
                    );

                    if (SyntaxUtil.GetValueType(tmp.Type) == ValueType.Unknown)
                        calculationExpression = EnsureJs(calculationExpression);

                    if (isPrefix)
                    {
                        return Expression.Block(
                            tmp.Type,
                            new[] { tmp },
                            Expression.Assign(tmp, source),
                            Expression.Assign(tmp, calculationExpression),
                            BuildSet(syntax.Operand, tmp),
                            tmp
                        );
                    }
                    else
                    {
                        return Expression.Block(
                            tmp.Type,
                            new[] { tmp },
                            Expression.Assign(tmp, source),
                            BuildSet(syntax.Operand, calculationExpression),
                            tmp
                        );
                    }

                case SyntaxExpressionType.Void:
                    return Expression.Block(
                        syntax.Operand.Accept(this),
                        Expression.Field(null, typeof(JsBox).GetField("Undefined"))
                    );

                case SyntaxExpressionType.Delete:
                    switch (syntax.Operand.Type)
                    {
                        case SyntaxType.Property:
                            var propertySyntax = (PropertySyntax)syntax.Operand;

                            return BuildDeleteMember(
                                propertySyntax.Expression.Accept(this),
                                propertySyntax.Name
                            );

                        case SyntaxType.Indexer:
                            var indexerSyntax = (IndexerSyntax)syntax.Operand;

                            return BuildDeleteIndex(
                                indexerSyntax.Expression.Accept(this),
                                indexerSyntax.Index.Accept(this)
                            );

                        case SyntaxType.Identifier:
                            var identifierSyntax = (IdentifierSyntax)syntax.Operand;

                            if (identifierSyntax.Target.Type == VariableType.Global)
                            {
                                return BuildDeleteMember(
                                    Expression.Property(
                                        _scope.Runtime,
                                        "GlobalScope"
                                    ),
                                    identifierSyntax.Name
                                );
                            }
                            else
                            {
                                // Locals are never configurable.

                                return Expression.Constant(false);
                            }

                        default:
                            return Expression.Block(
                                syntax.Operand.Accept(this),
                                Expression.Constant(true)
                            );
                    }

                default:
                    if (
                        syntax.Operation == SyntaxExpressionType.TypeOf &&
                        syntax.Operand.Type == SyntaxType.Identifier
                    ) {
                        var identifierSyntax = (IdentifierSyntax)syntax.Operand;

                        if (identifierSyntax.Target.Type == VariableType.Global)
                        {
                            return Expression.Call(
                                typeof(JintRuntime).GetMethod("Operation_TypeOf", new[] { typeof(JsObject), typeof(string) }),
                                Expression.Property(
                                    _scope.Runtime,
                                    "GlobalScope"
                                ),
                                Expression.Constant(identifierSyntax.Name)
                            );
                        }
                    }

                    return BuildOperationCall(
                        syntax.Operation,
                        syntax.Operand.Accept(this)
                    );
            }
        }

        public Expression VisitValue(ValueSyntax syntax)
        {
            // We don't handle literal here because this allows us to have
            // the raw values in the expression tree; not the JsBox's.

            if (syntax.Value == null)
                return Expression.Field(null, typeof(JsBox).GetField("Null"));

            return Expression.Constant(syntax.Value);
        }

        public Expression VisitRegexp(RegexpSyntax syntax)
        {
            return Expression.Call(
                Expression.Property(_scope.Runtime, "Global"),
                typeof(JsGlobal).GetMethod("CreateRegExp", InstanceFlags, null, new[] { typeof(string), typeof(string) }, null),
                Expression.Constant(syntax.Regexp),
                Expression.Constant(syntax.Options)
            );
        }

        public Expression VisitLabel(LabelSyntax syntax)
        {
            _labels.Add(syntax.Expression, syntax.Label);

            return syntax.Expression.Accept(this);
        }
    }
}
