using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Jint.Expressions;
using Jint.Native;
using Jint.Runtime;

namespace Jint.Backend.Dlr
{
    internal partial class ExpressionVisitor : ISyntaxVisitor<Expression>
    {
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
                return _scope.BuildSet(syntax.Left, syntax.Right.Accept(this));

            ExpressionType expressionType;

            switch (syntax.AssignmentOperator)
            {
                case AssignmentOperator.Add: expressionType = ExpressionType.Add; break;
                default: throw new NotImplementedException();
            }

            return _scope.BuildSet(
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public Expression VisitFunction(FunctionSyntax syntax)
        {
            return CreateFunctionSyntax(syntax, DeclareFunction(syntax));
        }

        private Expression CreateFunctionSyntax(IFunctionDeclaration function, DlrFunctionDelegate compiledFunction)
        {
            Expression parameters;

            if (function.Parameters.Count == 0)
            {
                parameters = Expression.Constant(null, typeof(string[]));
            }
            else
            {
                parameters = Expression.NewArrayInit(
                    typeof(string),
                    function.Parameters.Select(Expression.Constant)
                );
            }

            return Expression.Call(
                _scope.Runtime,
                typeof(JintRuntime).GetMethod("CreateFunction"),
                Expression.Constant(function.Name, typeof(string)),
                Expression.Constant(compiledFunction),
                _scope.ClosureLocal != null ? (Expression)_scope.ClosureLocal : Expression.Constant(null),
                parameters
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
                var parameter = function.Parameters[i];
                var local = Expression.Parameter(
                    typeof(JsInstance),
                    parameter
                );
                locals.Add(local);

                _scope.Variables.Add(body.DeclaredVariables[parameter], local);

                statements.Add(Expression.Assign(
                    local,
                    Expression.Condition(
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
                    )
                ));
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
            Expression arguments;

            if (syntax.Arguments.Count > 0)
            {
                arguments = Expression.NewArrayInit(
                    typeof(JsInstance),
                    syntax.Arguments.Select(p => p.Accept(this))
                );
            }
            else
            {
                arguments = Expression.Constant(null, typeof(JsInstance[]));
            }

            return Expression.Call(
                _scope.Runtime,
                typeof(JintRuntime).GetMethod("ExecuteFunction"),
                Expression.Constant(null, typeof(JsInstance)), // Call target (this)
                _scope.BuildGet(syntax.Expression),
                arguments
            );
        }

        public Expression VisitIndexer(IndexerSyntax syntax)
        {
            return _scope.BuildGet(syntax);
        }

        public Expression VisitProperty(PropertySyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitPropertyDeclaration(PropertyDeclarationSyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitIdentifier(IdentifierSyntax syntax)
        {
            return _scope.BuildGet(syntax);
        }

        public Expression VisitJsonExpression(JsonExpressionSyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitNew(NewSyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitBinaryExpression(BinaryExpressionSyntax syntax)
        {
            switch (syntax.Type)
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
                    return Expression.Call(
                        _scope.Runtime,
                        typeof(JintRuntime).GetMethod("BinaryOperation"),
                        syntax.Left.Accept(this),
                        syntax.Right.Accept(this),
                        Expression.Constant(syntax.Type)
                    );
            }

            ExpressionType expressionType;

            switch (syntax.Type)
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
                case BinaryExpressionType.NotSame: expressionType = ExpressionType.And; break;
                case BinaryExpressionType.Same: expressionType = ExpressionType.And; break;
                case BinaryExpressionType.UnsignedRightShift: expressionType = ExpressionType.RightShift; break;
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
            switch (syntax.Type)
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
                    bool isIncrement = syntax.Type == UnaryExpressionType.PrefixPlusPlus || syntax.Type == UnaryExpressionType.PostfixPlusPlus;
                    bool isPrefix = syntax.Type == UnaryExpressionType.PrefixMinusMinus || syntax.Type == UnaryExpressionType.PrefixPlusPlus;

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
                            Expression.Assign(tmp, _scope.BuildGet(syntax.Operand)),
                            Expression.Assign(tmp, calculationExpression),
                            _scope.BuildSet(syntax.Operand, tmp),
                            tmp
                        );
                    }
                    else
                    {
                        return Expression.Block(
                            typeof(JsInstance),
                            new[] { tmp },
                            Expression.Assign(tmp, _scope.BuildGet(syntax.Operand)),
                            _scope.BuildSet(syntax.Operand, calculationExpression),
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

            switch (syntax.Type)
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
    }
}
