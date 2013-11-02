using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var runtimeParameter = Expression.Parameter(
                typeof(JintRuntime),
                RuntimeParameterName
            );

            _scope = new Scope(runtimeParameter, null, null);

            var statements = new List<Expression>
            {
                ProcessFunctionBody(syntax, runtimeParameter)
            };

            return Expression.Lambda<Func<JintRuntime, JsInstance>>(
                Expression.Block(statements),
                new[] { runtimeParameter }
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
            throw new NotImplementedException();
        }

        public Expression VisitBlock(BlockSyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitBreak(BreakSyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitContinue(ContinueSyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitDoWhile(DoWhileSyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitEmpty(EmptySyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitExpressionStatement(ExpressionStatementSyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitForEachIn(ForEachInSyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitFor(ForSyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitFunctionDeclaration(FunctionDeclarationSyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitIf(IfSyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitReturn(ReturnSyntax syntax)
        {
            throw new NotImplementedException();
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

            switch (syntax.Target.Type)
            {
                case VariableType.Local:
                case VariableType.Parameter:
                case VariableType.Arguments:
                    return Expression.Assign(
                        _scope.ResolveVariable(syntax.Target),
                        syntax.Expression.Accept(this)
                    );

                case VariableType.Global:
                    return Expression.Convert(
                        Expression.Dynamic(
                            _context.SetMember(syntax.Identifier),
                            typeof(object),
                            Expression.Property(
                                _scope.Runtime,
                                JintRuntime.GlobalScopeName
                            ),
                            syntax.Expression.Accept(this)
                        ),
                        typeof(JsInstance)
                    );

                default:
                    throw new InvalidOperationException("Cannot assign");
            }
        }

        public Expression VisitWhile(WhileSyntax syntax)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public Expression VisitMemberAccess(MemberAccessSyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitMethodCall(MethodCallSyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitIndexer(IndexerSyntax syntax)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public Expression VisitTernary(TernarySyntax syntax)
        {
            throw new NotImplementedException();
        }

        public Expression VisitUnaryExpression(UnaryExpressionSyntax syntax)
        {
            throw new NotImplementedException();
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
