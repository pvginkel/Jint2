using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Jint.ExpressionExtensions;
using Jint.Expressions;
using Jint.Native;
using Jint.Runtime;

namespace Jint.Backend.Dlr
{
    internal partial class ExpressionVisitor : ISyntaxVisitor<Expression>
    {
        private static readonly MethodInfo _defineAccessorProperty = typeof(JsDictionaryObject).GetMethod("DefineAccessorProperty");
        private static readonly ConstructorInfo _argumentsConstructor = typeof(JsArguments).GetConstructors().Single();
        private static readonly MethodInfo _compareEquality = typeof(JintRuntime).GetMethod("CompareEquality");

        private Scope _scope;
        private readonly Dictionary<SyntaxNode, string> _labels = new Dictionary<SyntaxNode, string>();

        private const string RuntimeParameterName = "<>runtime";

        public Expression VisitProgram(ProgramSyntax syntax)
        {
            var statements = new List<Expression>();
            var locals = new List<ParameterExpression>();
            
            var that = Expression.Parameter(typeof(JsInstance), "this");

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
                Expression.Property(
                    _scope.Runtime,
                    "Global"
                )
            ));

            // Build the body and return label.

            var body = ProcessFunctionBody(syntax, _scope.Runtime);

            if (body.Type == typeof(void))
            {
                statements.Add(body);
                statements.Add(Expression.Label(
                    _scope.Return,
                    Expression.Constant(JsUndefined.Instance)
                ));
            }
            else
            {
                statements.Add(Expression.Label(
                    _scope.Return,
                    body
                ));
            }

            return Expression.Lambda<Func<JintRuntime, JsInstance>>(
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
                if (variable.Type == Expressions.VariableType.Local)
                {
                    var parameter = Expression.Parameter(
                        typeof(JsInstance),
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

            SyntaxExpressionType operation;

            switch (syntax.Operation)
            {
                case AssignmentOperator.Add: operation = SyntaxExpressionType.Add; break;
                case AssignmentOperator.BitwiseAnd: operation = SyntaxExpressionType.BitwiseAnd; break;
                case AssignmentOperator.Divide: operation = SyntaxExpressionType.Divide; break;
                case AssignmentOperator.Modulo: operation = SyntaxExpressionType.Modulo; break;
                case AssignmentOperator.Multiply: operation = SyntaxExpressionType.Multiply; break;
                case AssignmentOperator.BitwiseOr: operation = SyntaxExpressionType.BitwiseOr; break;
                case AssignmentOperator.LeftShift: operation = SyntaxExpressionType.LeftShift; break;
                case AssignmentOperator.RightShift: operation = SyntaxExpressionType.RightShift; break;
                case AssignmentOperator.Subtract: operation = SyntaxExpressionType.Subtract; break;
                case AssignmentOperator.UnsignedRightShift: operation = SyntaxExpressionType.UnsignedRightShift; break;
                case AssignmentOperator.BitwiseExclusiveOr: operation = SyntaxExpressionType.BitwiseExclusiveOr; break;
                default: throw new NotImplementedException();
            }

            return BuildSet(
                syntax.Left,
                new BinaryExpressionSyntax(
                    operation,
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
                        BuildToBoolean(syntax.Test.Accept(this)),
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

            var keyLocal = Expression.Parameter(typeof(JsInstance), "key");

            var result = ExpressionEx.ForEach(
                keyLocal,
                typeof(JsInstance),
                // Call JintRuntime.GetForEachKeys to get the keys to enumerate over.
                Expression.Call(
                    _scope.Runtime,
                    typeof(JintRuntime).GetMethod("GetForEachKeys"),
                    syntax.Expression.Accept(this)
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
                    BuildToBoolean(test),
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
            var compiledFunction = DeclareFunction(syntax);

            return _scope.BuildSet(
                syntax.Target,
                CreateFunctionSyntax(syntax, compiledFunction)
            );
        }

        public Expression VisitIf(IfSyntax syntax)
        {
            return Expression.IfThenElse(
                BuildToBoolean(syntax.Test.Accept(this)),
                syntax.Then != null ? syntax.Then.Accept(this) : Expression.Empty(),
                syntax.Else != null ? syntax.Else.Accept(this) : Expression.Empty()
            );
        }

        public Expression VisitReturn(ReturnSyntax syntax)
        {
            return Expression.Goto(
                _scope.Return,
                syntax.Expression != null
                ? syntax.Expression.Accept(this)
                : Expression.Default(typeof(JsInstance))
            );
        }

        public Expression VisitSwitch(SwitchSyntax syntax)
        {
            // Create the label that jumps to the end of the switch.
            var after = Expression.Label(GetLabel(syntax) ?? "<>after");
            _scope.BreakTargets.Push(after);

            var statements = new List<Expression>();

            var expression = Expression.Parameter(typeof(JsInstance), "expression");

            statements.Add(Expression.Assign(
                expression,
                syntax.Expression.Accept(this)
            ));

            var bodies = new List<Tuple<LabelTarget, Expression>>();

            foreach (var caseClause in syntax.Cases)
            {
                var target = Expression.Label("target" + bodies.Count);

                statements.Add(Expression.IfThen(
                    Expression.Call(
                        _scope.Runtime,
                        _compareEquality,
                        expression,
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
                new[] { expression },
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
                    typeof(JsException).GetConstructor(new[] { typeof(JsInstance) }),
                    syntax.Expression.Accept(this)
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
                        BuildToBoolean(syntax.Test.Accept(this)),
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

            var array = Expression.Parameter(typeof(JsArray), "array");

            statements.Add(Expression.Assign(
                array,
                Expression.Call(
                    Expression.Property(
                        Expression.Property(
                            _scope.Runtime,
                            typeof(JintRuntime).GetProperty("Global")
                        ),
                        typeof(JsGlobal).GetProperty("ArrayClass")
                    ),
                    typeof(JsArrayConstructor).GetMethod("New")
                )
            ));

            for (int i = 0; i < syntax.Parameters.Count; i++)
            {
                statements.Add(BuildSetIndex(
                    array,
                    BuildNewString(Expression.Constant(i.ToString(CultureInfo.InvariantCulture))),
                    syntax.Parameters[i].Accept(this)
                ));
            }

            statements.Add(array);

            return Expression.Block(
                typeof(JsInstance),
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
            return CreateFunctionSyntax(syntax, DeclareFunction(syntax));
        }

        private Expression CreateFunctionSyntax(IFunctionDeclaration function, DlrFunctionDelegate compiledFunction)
        {
            return Expression.Call(
                _scope.Runtime,
                typeof(JintRuntime).GetMethod("CreateFunction"),
                Expression.Constant(function.Name, typeof(string)),
                Expression.Constant(compiledFunction),
                _scope.ClosureLocal != null ? (Expression)_scope.ClosureLocal : Expression.Constant(null),
                MakeArrayInit(function.Parameters.Select(Expression.Constant), typeof(string), true)
            );
        }

        public DlrFunctionDelegate DeclareFunction(IFunctionDeclaration function)
        {
            var body = function.Body;

            var thisParameter = Expression.Parameter(
                typeof(JsDictionaryObject),
                "this"
            );

            var functionParameter = Expression.Parameter(
                typeof(JsFunction),
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
                typeof(JsInstance[]),
                "argumentsParameter"
            );

            var statements = new List<Expression>();

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
                argumentsParameter
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
                    (declaredVariable.Type == Expressions.VariableType.Local || declaredVariable.Type == Expressions.VariableType.Arguments) &&
                    declaredVariable.ClosureField == null
                ) {
                    var local = Expression.Parameter(
                        typeof(JsInstance),
                        declaredVariable.Name
                    );
                    locals.Add(local);

                    statements.Add(Expression.Assign(
                        local,
                        Expression.Constant(JsUndefined.Instance)
                    ));

                    _scope.Variables.Add(declaredVariable, local);
                }
            }

            // Initialize the arguments array.

            statements.Add(_scope.BuildSet(
                _scope.ArgumentsVariable,
                Expression.New(
                    _argumentsConstructor,
                    Expression.Property(_scope.Runtime, typeof(JintRuntime).GetProperty("Global")),
                    functionParameter,
                    argumentsParameter
                )
            ));

            // Build the body.

            statements.Add(body.Accept(this));

            // Add the return label.

            statements.Add(Expression.Label(
                _scope.Return,
                Expression.Constant(JsUndefined.Instance)
            ));

            // Add all gathered locals for the closures to the locals list.

            foreach (var local in _scope.ClosureLocals.Values)
            {
                locals.Add(local);
            }

            // Add the arguments if one was created.

            var lambda = Expression.Lambda<DlrFunctionDelegate>(
                Expression.Block(
                    typeof(JsInstance), // TODO: Switch to DlrFunctionResult
                    locals,
                    statements
                ),
                parameters
            );

            _scope = _scope.Parent;

            DlrBackend.PrintExpression(lambda);

            return lambda.Compile();
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

                target = Expression.Parameter(typeof(JsInstance), "target");

                if (memberSyntax.Type == SyntaxType.Property)
                    getter = BuildGetMember(target, ((PropertySyntax)memberSyntax).Name);
                else
                    getter = BuildGetIndex(target, BuildGet(((IndexerSyntax)memberSyntax).Index));

                statements.Add(Expression.Assign(target, memberSyntax.Expression.Accept(this)));
                parameters.Add((ParameterExpression)target);
            }
            else
            {
                var identifierSyntax = syntax.Expression as IdentifierSyntax;

                if (
                    identifierSyntax != null &&
                    identifierSyntax.Target.Type == Expressions.VariableType.WithScope
                )
                {
                    // With a with scope, the target depends on how the variable
                    // is resolved.

                    var withTarget = Expression.Parameter(typeof(JsInstance), "target");
                    statements.Add(Expression.Assign(
                        withTarget,
                        Expression.Constant(null, typeof(JsInstance))
                    ));

                    var method = Expression.Parameter(
                        typeof(JsInstance),
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
                    target = Expression.Constant(null, typeof(JsInstance));
                    getter = BuildGet(syntax.Expression);
                }
            }

            var arguments = Expression.Parameter(
                typeof(JsInstance[]),
                "arguments"
            );
            parameters.Add(arguments);
            var result = Expression.Parameter(
                typeof(JsInstance),
                "result"
            );
            parameters.Add(result);
            var outParameters = Expression.Parameter(
                typeof(bool[]),
                "outParameters"
            );
            parameters.Add(outParameters);

            var expressions = new List<Expression>();
            bool anyAssignable = false;

            foreach (var expression in syntax.Arguments)
            {
                if (expression.IsAssignable)
                    anyAssignable = true;
                expressions.Add(expression.Accept(this));
            }

            statements.Add(Expression.Assign(
                arguments,
                MakeArrayInit(expressions, typeof(JsInstance), true)
            ));

            statements.Add(Expression.Assign(
                result,
                Expression.Call(
                    _scope.Runtime,
                    typeof(JintRuntime).GetMethod("ExecuteFunction"),
                    target,
                    getter,
                    arguments,
                    MakeArrayInit(syntax.Generics, typeof(JsInstance), true),
                    outParameters
                )
            ));

            // We need to read the arguments back for when the ExecuteFunction
            // has out parameters for native calls.

            if (anyAssignable)
            {
                var writeBackStatements = new List<Expression>();

                for (int i = 0; i < syntax.Arguments.Count; i++)
                {
                    var argument = syntax.Arguments[i];

                    if (argument.IsAssignable)
                    {
                        writeBackStatements.Add(Expression.IfThen(
                            Expression.IsTrue(
                                Expression.ArrayIndex(outParameters, Expression.Constant(i))
                            ),
                            BuildSet(
                                argument,
                                Expression.ArrayIndex(
                                    arguments,
                                    Expression.Constant(i)
                                )
                            )
                        ));
                    }
                }

                statements.Add(Expression.IfThen(
                    Expression.NotEqual(
                        outParameters,
                        Expression.Constant(null)
                    ),
                    Expression.Block(
                        writeBackStatements
                    )
                ));
            }

            statements.Add(result);

            return Expression.Block(
                typeof(JsInstance),
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

        public Expression VisitPropertyDeclaration(PropertyDeclarationSyntax syntax)
        {
            throw new InvalidOperationException("Property declaration must be handled in VisitJsonExpression");
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
                        Expression.Property(
                            global,
                            typeof(JsGlobal).GetProperty("ObjectClass")
                        ),
                        typeof(JsObjectConstructor).GetMethod("New", new Type[0])
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
                            global,
                            Expression.Constant(accessorProperty.Name),
                            accessorProperty.GetExpression != null ? accessorProperty.GetExpression.Accept(this) : Expression.Default(typeof(JsFunction)),
                            accessorProperty.SetExpression != null ? accessorProperty.SetExpression.Accept(this) : Expression.Default(typeof(JsFunction))
                        }
                    ));
                }
            }

            statements.Add(obj);

            return Expression.Block(
                typeof(JsInstance),
                new[] { obj, global },
                statements
            );
        }

        public Expression VisitNew(NewSyntax syntax)
        {
            var methodCall = syntax.Expression as MethodCallSyntax;

            IList<ExpressionSyntax> arguments = null;
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
                MakeArrayInit(arguments, typeof(JsInstance), true),
                MakeArrayInit(generics, typeof(JsInstance), true)
            );
        }

        private Expression MakeArrayInit(IEnumerable<SyntaxNode> initializers, Type elementType, bool nullWhenEmpty)
        {
            if (initializers == null)
                initializers = new SyntaxNode[0];

            return MakeArrayInit(initializers.Select(p => p.Accept(this)), elementType, nullWhenEmpty);
        }

        public static Expression MakeArrayInit(IEnumerable<Expression> initializers, Type elementType, bool nullWhenEmpty)
        {
            var expressions = initializers.ToList();

            if (expressions.Count == 0)
            {
                if (nullWhenEmpty)
                    return Expression.Constant(null, elementType.MakeArrayType());

                return Expression.NewArrayBounds(elementType, Expression.Constant(0));
            }

            return Expression.NewArrayInit(elementType, expressions);

        }

        public Expression VisitBinaryExpression(BinaryExpressionSyntax syntax)
        {
            switch (syntax.Operation)
            {
                case SyntaxExpressionType.And:
                    var tmp = Expression.Parameter(typeof(JsInstance), "tmp");

                    return Expression.Block(
                        typeof(JsInstance),
                        new[] { tmp },
                        Expression.Assign(tmp, syntax.Left.Accept(this)),
                        Expression.Condition(
                            BuildToBoolean(tmp),
                            syntax.Right.Accept(this),
                            tmp,
                            typeof(JsInstance)
                        )
                    );

                case SyntaxExpressionType.Or:
                    tmp = Expression.Parameter(typeof(JsInstance), "tmp");

                    return Expression.Block(
                        typeof(JsInstance),
                        new[] { tmp },
                        Expression.Assign(tmp, syntax.Left.Accept(this)),
                        Expression.Condition(
                            BuildToBoolean(tmp),
                            tmp,
                            syntax.Right.Accept(this),
                            typeof(JsInstance)
                        )
                    );

                default:
                    return Expression.Call(
                        _scope.Runtime,
                        typeof(JintRuntime).GetMethod("Operation_" + syntax.Operation),
                        syntax.Left.Accept(this),
                        syntax.Right.Accept(this)
                    );
            }
        }

        public Expression VisitTernary(TernarySyntax syntax)
        {
            return Expression.Condition(
                BuildToBoolean(syntax.Test.Accept(this)),
                syntax.Then.Accept(this),
                syntax.Else.Accept(this),
                typeof(JsInstance)
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

                    var tmp = Expression.Parameter(
                        typeof(JsInstance),
                        "tmp"
                    );

                    var calculationExpression = BuildNewNumber(
                        Expression.MakeBinary(
                            isIncrement ? ExpressionType.Add : ExpressionType.Subtract,
                            BuildToNumber(tmp),
                            Expression.Constant(1d)
                        )
                    );

                    if (isPrefix)
                    {
                        return Expression.Block(
                            typeof(JsInstance),
                            new[] { tmp },
                            Expression.Assign(tmp, BuildGet(syntax.Operand)),
                            Expression.Assign(tmp, calculationExpression),
                            BuildSet(syntax.Operand, tmp),
                            tmp
                        );
                    }
                    else
                    {
                        return Expression.Block(
                            typeof(JsInstance),
                            new[] { tmp },
                            Expression.Assign(tmp, BuildGet(syntax.Operand)),
                            BuildSet(syntax.Operand, calculationExpression),
                            tmp
                        );
                    }

                case SyntaxExpressionType.Void:
                    return Expression.Block(
                        syntax.Operand.Accept(this),
                        Expression.Constant(JsUndefined.Instance)
                    );

                case SyntaxExpressionType.Delete:
                    var operand = (MemberSyntax)syntax.Operand;

                    if (operand.Type == SyntaxType.Property)
                    {
                        return BuildDeleteMember(
                            operand.Expression.Accept(this),
                            ((PropertySyntax)operand).Name
                        );
                    }
                    else
                    {
                        return BuildDeleteIndex(
                            operand.Expression.Accept(this),
                            ((IndexerSyntax)operand).Index.Accept(this)
                        );
                    }

                default:
                    return Expression.Call(
                        _scope.Runtime,
                        typeof(JintRuntime).GetMethod("Operation_" + syntax.Operation),
                        syntax.Operand.Accept(this)
                    );
            }
        }

        public Expression VisitValue(ValueSyntax syntax)
        {
            if (syntax.Value == null)
            {
                return Expression.Property(
                    null,
                    typeof(JsNull).GetProperty("Instance")
                );
            }

            string klass;
            MethodInfo method;

            switch (syntax.TypeCode)
            {
                case TypeCode.Boolean:
                    klass = "BooleanClass";
                    method = typeof(JsBooleanConstructor).GetMethod("New", new[] { syntax.Value.GetType() });
                    break;

                case TypeCode.Int32:
                case TypeCode.Single:
                case TypeCode.Double:
                    klass = "NumberClass";
                    method = typeof(JsNumberConstructor).GetMethod("New", new[] { syntax.Value.GetType() });
                    break;

                case TypeCode.String:
                    klass = "StringClass";
                    method = typeof(JsStringConstructor).GetMethod("New", new[] { syntax.Value.GetType() });
                    break;

                default:
                    Debug.Assert(syntax.Value is JsInstance);
                    return Expression.Constant(syntax.Value);
            }

            return Expression.Call(
                Expression.Property(
                    Expression.Property(
                        _scope.Runtime,
                        JintRuntime.GlobalName
                    ),
                    klass
                ),
                method,
                new Expression[] { Expression.Constant(syntax.Value) }
            );
        }

        public Expression VisitRegexp(RegexpSyntax syntax)
        {
            return Expression.Call(
                Expression.Property(
                    Expression.Property(
                        _scope.Runtime,
                        "Global"
                    ),
                    "RegExpClass"
                ),
                typeof(JsRegExpConstructor).GetMethod("New", new[] { typeof(string), typeof(bool), typeof(bool), typeof(bool) }),
                Expression.Constant(syntax.Regexp),
                Expression.Constant(syntax.Options.Contains("g")),
                Expression.Constant(syntax.Options.Contains("i")),
                Expression.Constant(syntax.Options.Contains("m"))
            );
        }

        public Expression VisitClrIdentifier(ClrIdentifierSyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitLabel(LabelSyntax syntax)
        {
            _labels.Add(syntax.Expression, syntax.Label);

            return syntax.Expression.Accept(this);
        }
    }
}
