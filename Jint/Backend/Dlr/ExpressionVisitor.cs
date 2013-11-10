﻿using System;
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

        private readonly JintContext _context;
        private Scope _scope;

        private const string RuntimeParameterName = "<>runtime";

        public ExpressionVisitor(JintContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            _context = context;
        }

        public Expression VisitProgram(ProgramSyntax syntax)
        {
            var statements = new List<Expression>();

            _scope = new Scope(this, null, null, null, null, statements, null);

            statements.Add(ProcessFunctionBody(syntax, _scope.Runtime));

            statements.Add(Expression.Label(
                _scope.Return,
                Expression.Constant(JsUndefined.Instance)
            ));

            return Expression.Lambda<Func<JintRuntime, JsInstance>>(
                Expression.Block(statements),
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
            if (syntax.AssignmentOperator == AssignmentOperator.Assign)
                return BuildSet(syntax.Left, syntax.Right.Accept(this));

            ExpressionType expressionType;

            switch (syntax.AssignmentOperator)
            {
                case AssignmentOperator.Add: expressionType = ExpressionType.Add; break;
                default: throw new NotImplementedException();
            }

            return BuildSet(
                syntax.Left,
                Expression.Dynamic(
                    _context.Convert(typeof(JsInstance), true),
                    typeof(JsInstance),
                    Expression.Dynamic(
                        _context.BinaryOperation(expressionType),
                        typeof(object),
                        syntax.Left.Accept(this),
                        syntax.Right.Accept(this)
                    )
                )
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
            if (_scope.BreakTargets.Count == 0)
                throw new InvalidOperationException("There is nothing to break to");

            return Expression.Goto(_scope.BreakTargets.Peek());
        }

        public Expression VisitContinue(ContinueSyntax syntax)
        {
            if (_scope.ContinueTargets.Count == 0)
                throw new InvalidOperationException("There is nothing to continue to");

            return Expression.Goto(_scope.ContinueTargets.Peek());
        }

        public Expression VisitDoWhile(DoWhileSyntax syntax)
        {
            throw new NotImplementedException();
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

            var breakTarget = Expression.Label("break");
            _scope.BreakTargets.Push(breakTarget);
            var continueTarget = Expression.Label("continue");
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

            // Push the break and continue targets onto the stack.

            var breakTarget = Expression.Label("break");
            _scope.BreakTargets.Push(breakTarget);
            var continueTarget = Expression.Label("continue");
            _scope.ContinueTargets.Push(continueTarget);

            var loopStatements = new List<Expression>();

            // If we have a test, we perform the test at the start of every
            // iteration. The test is itself an expression which we here
            // convert to a bool to be able to test it. The else branch
            // is used to invert the result.

            if (test != null)
            {
                loopStatements.Add(Expression.IfThenElse(
                    Expression.Dynamic(
                        _context.Convert(
                            typeof(bool),
                            true
                        ),
                        typeof(bool),
                        test
                    ),
                    Expression.Empty(),
                    Expression.Goto(breakTarget)
                ));
            }

            // Add the body.

            if (body != null)
                loopStatements.Add(body);

            // Increment is done at the end.

            if (increment != null)
                loopStatements.Add(increment);

            statements.Add(Expression.Loop(
                Expression.Block(
                    loopStatements
                ),
                breakTarget,
                continueTarget
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
                Expression.Dynamic(
                    _context.Convert(typeof(bool), true),
                    typeof(bool),
                    syntax.Test.Accept(this)
                ),
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
            throw new NotImplementedException();
        }

        public Expression VisitWith(WithSyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitThrow(ThrowSyntax syntax)
        {
            throw new NotImplementedException();
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
                        Expression.Block(catchStatements),
                        null
                    )
                };
            }

            return Expression.TryCatchFinally(
                syntax.Body.Accept(this),
                syntax.Finally != null ? syntax.Finally.Body.Accept(this) : Expression.Empty(),
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

            var breakTarget = Expression.Label("break");
            _scope.BreakTargets.Push(breakTarget);
            var continueTarget = Expression.Label("continue");
            _scope.ContinueTargets.Push(continueTarget);

            var result = Expression.Loop(
                Expression.Block(
                    typeof(void),
                    // At the beginning of every iteration, perform the test.
                    Expression.IfThenElse(
                        Expression.Dynamic(
                            _context.Convert(typeof(bool), true),
                            typeof(bool),
                            syntax.Test.Accept(this)
                        ),
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
            throw new NotImplementedException();
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

        private DlrFunctionDelegate DeclareFunction(IFunctionDeclaration function)
        {
            var body = function.Body;

            var thisParameter = Expression.Parameter(
                typeof(JsDictionaryObject),
                "this"
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
                JsScope.Arguments
            );

            var statements = new List<Expression>();

            _scope = new Scope(this, thisParameter, scopedClosure, closureLocal, argumentsParameter, statements, _scope);

            var closureParameter = Expression.Parameter(
                typeof(object),
                "closureParameter"
            );

            var locals = new List<ParameterExpression>();
            var parameters = new List<ParameterExpression>
            {
                _scope.Runtime,
                thisParameter,
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

            // Copy the function parameters.

            for (int i = 0; i < function.Parameters.Count; i++)
            {
                var assignment = Expression.Condition(
                    Expression.MakeBinary(
                        ExpressionType.GreaterThan,
                        Expression.ArrayLength(argumentsParameter),
                        Expression.Constant(i)
                    ),
                    Expression.ArrayAccess(
                        argumentsParameter,
                        Expression.Constant(i)
                    ),
                    Expression.Constant(JsUndefined.Instance),
                    typeof(JsInstance)
                );

                string parameter = function.Parameters[i];
                var declaredVariable = body.DeclaredVariables[parameter];

                if (declaredVariable.ClosureField == null)
                {
                    var local = Expression.Parameter(
                        typeof(JsInstance),
                        parameter
                    );
                    locals.Add(local);

                    _scope.Variables.Add(declaredVariable, local);

                    statements.Add(Expression.Assign(
                        local,
                        assignment
                    ));
                }
                else
                {
                    statements.Add(_scope.BuildSet(
                        declaredVariable,
                        assignment
                    ));
                }
            }

            // Build the locals.

            foreach (var declaredVariable in body.DeclaredVariables)
            {
                if (
                    declaredVariable.Type == VariableType.Local &&
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

            // Build the body.

            statements.Add(body.Accept(this));

            // Add in the return label.

            statements.Add(Expression.Label(
                _scope.Return,
                Expression.Constant(JsUndefined.Instance)
            ));

            // Add all gathered locals for the closures to the locals list.

            foreach (var local in _scope.ClosureLocals.Values)
            {
                locals.Add(local);
            }

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
            var memberSyntax = syntax.Expression as MemberSyntax;
            if (memberSyntax != null)
            {
                // We need to get a hold on the object we need to execute on.
                // This applies to an index and property. The target is stored in
                // a local and both the getter and the ExecuteFunction is this
                // local.

                var target = Expression.Parameter(typeof(JsInstance), "target");

                Expression getter;

                if (memberSyntax.Type == SyntaxType.Property)
                {
                    getter = Expression.Convert(
                        Expression.Dynamic(
                            _context.GetMember(((PropertySyntax)memberSyntax).Name),
                            typeof(object),
                            target
                        ),
                        typeof(JsInstance)
                    );
                }
                else
                {
                    getter = Expression.Convert(
                        Expression.Dynamic(
                            _context.GetIndex(new CallInfo(0)),
                            typeof(object),
                            BuildGet(((IndexerSyntax)memberSyntax).Index),
                            target
                        ),
                        typeof(JsInstance)
                    );
                }

                return Expression.Block(
                    typeof(JsInstance),
                    new[] { target },
                    Expression.Assign(target, memberSyntax.Expression.Accept(this)),
                    Expression.Call(
                        _scope.Runtime,
                        typeof(JintRuntime).GetMethod("ExecuteFunction"),
                        target,
                        getter,
                        MakeArrayInit(syntax.Arguments, typeof(JsInstance), true)
                    )
                );
            }

            return Expression.Call(
                _scope.Runtime,
                typeof(JintRuntime).GetMethod("ExecuteFunction"),
                Expression.Constant(null, typeof(JsInstance)),
                BuildGet(syntax.Expression),
                MakeArrayInit(syntax.Arguments, typeof(JsInstance), true)
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

            foreach (var expression in syntax.Values)
            {
                var declaration = expression.Value;

                switch (declaration.Mode)
                {
                    case PropertyExpressionType.Data:
                        statements.Add(Expression.Dynamic(
                            _context.SetMember(expression.Key),
                            typeof(object),
                            obj,
                            declaration.Expression.Accept(this)
                        ));
                        break;

                    default:
                        statements.Add(Expression.Call(
                            obj,
                            _defineAccessorProperty,
                            new[]
                            {
                                global,
                                Expression.Constant(expression.Key),
                                declaration.GetExpression != null ? declaration.GetExpression.Accept(this) : Expression.Default(typeof(JsFunction)),
                                declaration.SetExpression != null ? declaration.SetExpression.Accept(this) : Expression.Default(typeof(JsFunction))
                            }
                        ));
                        break;
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
            return Expression.Call(
                _scope.Runtime,
                typeof(JintRuntime).GetMethod("New"),
                syntax.Expression.Accept(this),
                MakeArrayInit(syntax.Arguments, typeof(JsInstance), true),
                MakeArrayInit(syntax.Generics, typeof(JsInstance), true)
            );
        }

        private Expression MakeArrayInit(IEnumerable<SyntaxNode> initializers, Type elementType, bool nullWhenEmpty)
        {
            return MakeArrayInit(initializers.Select(p => p.Accept(this)), elementType, nullWhenEmpty);
        }

        private Expression MakeArrayInit(IEnumerable<Expression> initializers, Type elementType, bool nullWhenEmpty)
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
                case BinaryExpressionType.And:
                    var tmp = Expression.Parameter(typeof(JsInstance), "tmp");

                    return Expression.Block(
                        typeof(JsInstance),
                        new[] { tmp },
                        Expression.Assign(tmp, syntax.Left.Accept(this)),
                        Expression.Condition(
                            Expression.Dynamic(
                                _context.Convert(typeof(bool), true),
                                typeof(bool),
                                tmp
                            ),
                            syntax.Right.Accept(this),
                            tmp,
                            typeof(JsInstance)
                        )
                    );

                case BinaryExpressionType.Or:
                    tmp = Expression.Parameter(typeof(JsInstance), "tmp");

                    return Expression.Block(
                        typeof(JsInstance),
                        new[] { tmp },
                        Expression.Assign(tmp, syntax.Left.Accept(this)),
                        Expression.Condition(
                            Expression.Dynamic(
                                _context.Convert(typeof(bool), true),
                                typeof(bool),
                                tmp
                            ),
                            tmp,
                            syntax.Right.Accept(this),
                            typeof(JsInstance)
                        )
                    );

                case BinaryExpressionType.LeftShift:
                case BinaryExpressionType.RightShift:
                case BinaryExpressionType.UnsignedRightShift:
                case BinaryExpressionType.Same:
                case BinaryExpressionType.NotSame:
                case BinaryExpressionType.In:
                case BinaryExpressionType.InstanceOf:
                    return Expression.Call(
                        _scope.Runtime,
                        typeof(JintRuntime).GetMethod("BinaryOperation"),
                        syntax.Left.Accept(this),
                        syntax.Right.Accept(this),
                        Expression.Constant(syntax.Operation)
                    );
            }

            ExpressionType expressionType;

            switch (syntax.Operation)
            {
                case BinaryExpressionType.BitwiseAnd: expressionType = ExpressionType.And; break;
                case BinaryExpressionType.BitwiseOr: expressionType = ExpressionType.Or; break;
                case BinaryExpressionType.BitwiseXOr: expressionType = ExpressionType.ExclusiveOr; break;
                case BinaryExpressionType.Div: expressionType = ExpressionType.Divide; break;
                case BinaryExpressionType.Equal: expressionType = ExpressionType.Equal; break;
                case BinaryExpressionType.Greater: expressionType = ExpressionType.GreaterThan; break;
                case BinaryExpressionType.GreaterOrEqual: expressionType = ExpressionType.GreaterThanOrEqual; break;
                case BinaryExpressionType.LeftShift: expressionType = ExpressionType.LeftShift; break;
                case BinaryExpressionType.Lesser: expressionType = ExpressionType.LessThan; break;
                case BinaryExpressionType.LesserOrEqual: expressionType = ExpressionType.LessThanOrEqual; break;
                case BinaryExpressionType.Minus: expressionType = ExpressionType.Subtract; break;
                case BinaryExpressionType.Modulo: expressionType = ExpressionType.Modulo; break;
                case BinaryExpressionType.NotEqual: expressionType = ExpressionType.NotEqual; break;
                case BinaryExpressionType.Plus: expressionType = ExpressionType.Add; break;
                case BinaryExpressionType.Pow: expressionType = ExpressionType.Power; break;
                case BinaryExpressionType.RightShift: expressionType = ExpressionType.RightShift; break;
                case BinaryExpressionType.Times: expressionType = ExpressionType.Multiply; break;
                default: throw new InvalidOperationException();
            }

            /*
                case BinaryExpressionType.In: expressionType = ExpressionType.And; break;
                case BinaryExpressionType.InstanceOf: expressionType = ExpressionType.And; break;
            */

            return Expression.Dynamic(
                _context.Convert(typeof(JsInstance), true),
                typeof(JsInstance),
                Expression.Dynamic(
                    _context.BinaryOperation(expressionType),
                    typeof(object),
                    syntax.Left.Accept(this),
                    syntax.Right.Accept(this)
                )
            );
        }

        public Expression VisitTernary(TernarySyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitUnaryExpression(UnaryExpressionSyntax syntax)
        {
            switch (syntax.Operation)
            {
                    /*
                case UnaryExpressionType.TypeOf:
                    _result = Syntax.InvocationExpression(
                        Syntax.ParseName(syntax.Type.ToString()),
                        Syntax.ArgumentList(
                            Syntax.Argument((CSharpSyntax.ExpressionSyntax)operand)
                        )
                    );
                    break;

                case UnaryExpressionType.Not:
                    switch (syntax.Type)
                    {
                        case UnaryExpressionType.Not: op = PrefixUnaryOperator.Exclamation; break;
                        default: throw new InvalidOperationException();
                    }

                    _result = Syntax.InvocationExpression(
                        Syntax.ParseName("Global.BooleanClass.New"),
                        Syntax.ArgumentList(
                            Syntax.Argument(
                                Syntax.PrefixUnaryExpression(
                                    op,
                                    Syntax.InvocationExpression(
                                        Syntax.MemberAccessExpression(
                                            (CSharpSyntax.ExpressionSyntax)operand,
                                            "ToBoolean"
                                        ),
                                        Syntax.ArgumentList()
                                    )
                                )
                            )
                        )
                    );
                    break;

                case UnaryExpressionType.Positive:
                case UnaryExpressionType.Negate:
                    switch (syntax.Type)
                    {
                        case UnaryExpressionType.Positive: op = PrefixUnaryOperator.Plus; break;
                        case UnaryExpressionType.Negate: op = PrefixUnaryOperator.Minus; break;
                        default: throw new InvalidOperationException();
                    }

                    _result = Syntax.InvocationExpression(
                        Syntax.ParseName("Global.NumberClass.New"),
                        Syntax.ArgumentList(
                            Syntax.Argument(
                                Syntax.PrefixUnaryExpression(
                                    op,
                                    Syntax.InvocationExpression(
                                        Syntax.MemberAccessExpression((CSharpSyntax.ExpressionSyntax)operand, "ToNumber"),
                                        Syntax.ArgumentList()
                                    )
                                )
                            )
                        )
                    );
                    break;
                */
                case UnaryExpressionType.PostfixPlusPlus:
                case UnaryExpressionType.PostfixMinusMinus:
                case UnaryExpressionType.PrefixPlusPlus:
                case UnaryExpressionType.PrefixMinusMinus:
                    bool isIncrement = syntax.Operation == UnaryExpressionType.PrefixPlusPlus || syntax.Operation == UnaryExpressionType.PostfixPlusPlus;
                    bool isPrefix = syntax.Operation == UnaryExpressionType.PrefixMinusMinus || syntax.Operation == UnaryExpressionType.PrefixPlusPlus;

                    var tmp = Expression.Parameter(
                        typeof(JsInstance),
                        "tmp"
                    );

                    var calculationExpression = Expression.Dynamic(
                        _context.Convert(typeof(JsInstance), true),
                        typeof(JsInstance),
                        Expression.MakeBinary(
                            isIncrement ? ExpressionType.Add : ExpressionType.Subtract,
                            Expression.Dynamic(
                                _context.Convert(typeof(double), true),
                                typeof(double),
                                tmp
                            ),
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

                    /*
                case UnaryExpressionType.Delete:
                    throw new NotImplementedException();

                //member = expression.Expression as MemberExpression;
                //if (member == null)
                //    throw new InvalidOperationException("Delete is not implemented");
                //member.Previous.Accept(this);
                //EnsureIdentifierIsDefined(Result);
                //value = Result;
                //string propertyName = null;
                //if (member.Member is PropertyExpression)
                //    propertyName = ((PropertyExpression)member.Member).Text;
                //if (member.Member is Indexer)
                //{
                //    ((Indexer)member.Member).Index.Accept(this);
                //    propertyName = Result.ToString();
                //}
                //if (string.IsNullOrEmpty(propertyName))
                //    throw new JsException(Global.TypeErrorClass.New());
                //try
                //{
                //    ((JsDictionaryObject)value).Delete(propertyName);
                //}
                //catch (JintException)
                //{
                //    throw new JsException(Global.TypeErrorClass.New());
                //}
                //Result = value;
                //break;

                case UnaryExpressionType.Void:
                    syntax.Operand.Accept(this);

                    _result = Syntax.InvocationExpression(
                        Syntax.ParseName("Void"),
                        Syntax.ArgumentList(
                            Syntax.Argument((CSharpSyntax.ExpressionSyntax)_result)
                        )
                    );
                    break;

                case UnaryExpressionType.Inv:
                    _result = Syntax.InvocationExpression(
                        Syntax.ParseName("Global.NumberClass.New"),
                        Syntax.ArgumentList(
                            Syntax.Argument(
                                Syntax.BinaryExpression(
                                    BinaryOperator.Minus,
                                    Syntax.BinaryExpression(
                                        BinaryOperator.Minus,
                                        Syntax.LiteralExpression(0),
                                        Syntax.InvocationExpression(
                                            Syntax.MemberAccessExpression((CSharpSyntax.ExpressionSyntax)operand, "ToNumber"),
                                            Syntax.ArgumentList()
                                        )
                                    ),
                                    Syntax.LiteralExpression(1)
                                )
                            )
                        )
                    );
                    break;
                    */
            }

            ExpressionType expressionType;

            switch (syntax.Operation)
            {
                case UnaryExpressionType.Not: expressionType = ExpressionType.Not; break;
                default: throw new NotImplementedException();
            }

            return Expression.Convert(
                Expression.Dynamic(
                    _context.UnaryOperation(expressionType),
                    typeof(object),
                    syntax.Operand.Accept(this)
                ),
                typeof(JsInstance)
            );
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
            throw new NotImplementedException();
        }

        public Expression VisitClrIdentifier(ClrIdentifierSyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression BuildGet(SyntaxNode syntax)
        {
            switch (syntax.Type)
            {
                case SyntaxType.VariableDeclaration:
                    return _scope.BuildGet(((VariableDeclarationSyntax)syntax).Target);

                case SyntaxType.Identifier:
                    return _scope.BuildGet(((IdentifierSyntax)syntax).Target);

                case SyntaxType.MethodCall:
                    return ((MethodCallSyntax)syntax).Accept(this);

                case SyntaxType.Property:
                    var property = (PropertySyntax)syntax;

                    return Expression.Convert(
                        Expression.Dynamic(
                            _context.GetMember(property.Name),
                            typeof(object),
                            property.Expression.Accept(this)
                        ),
                        typeof(JsInstance)
                    );

                case SyntaxType.Indexer:
                    var indexer = (IndexerSyntax)syntax;

                    return Expression.Convert(
                        Expression.Dynamic(
                            _context.GetIndex(new CallInfo(0)),
                            typeof(object),
                            BuildGet(indexer.Expression),
                            indexer.Index.Accept(this)
                        ),
                        typeof(JsInstance)
                    );

                case SyntaxType.Function:
                    return syntax.Accept(this);

                default:
                    throw new NotImplementedException();
            }
        }

        public Expression BuildSet(SyntaxNode syntax, Expression value)
        {
            switch (syntax.Type)
            {
                case SyntaxType.VariableDeclaration:
                    return _scope.BuildSet(((VariableDeclarationSyntax)syntax).Target, value);

                case SyntaxType.Identifier:
                    return _scope.BuildSet(((IdentifierSyntax)syntax).Target, value);

                case SyntaxType.Property:
                    var property = (PropertySyntax)syntax;

                    return Expression.Convert(
                        Expression.Dynamic(
                            _context.SetMember(property.Name),
                            typeof(object),
                            property.Expression.Accept(this),
                            value
                        ),
                        typeof(JsInstance)
                    );

                case SyntaxType.Indexer:
                    var indexer = (IndexerSyntax)syntax;

                    return Expression.Convert(
                        Expression.Dynamic(
                            _context.SetIndex(new CallInfo(0)),
                            typeof(object),
                            BuildGet(indexer.Expression),
                            indexer.Index.Accept(this),
                            value
                        ),
                        typeof(JsInstance)
                    );

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
