using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    public class SyntaxVisitor : ISyntaxVisitor
    {
        public virtual void VisitProgram(ProgramSyntax syntax)
        {
            VisitBlock(syntax);
        }

        public virtual void VisitAssignment(AssignmentSyntax syntax)
        {
            syntax.Left.Accept(this);
            syntax.Right.Accept(this);
        }

        public virtual void VisitBlock(BlockSyntax syntax)
        {
            foreach (var statement in syntax.Statements)
            {
                statement.Accept(this);
            }
        }

        public virtual void VisitBreak(BreakSyntax syntax)
        {
        }

        public virtual void VisitContinue(ContinueSyntax syntax)
        {
        }

        public virtual void VisitDoWhile(DoWhileSyntax syntax)
        {
            syntax.Test.Accept(this);
            syntax.Body.Accept(this);
        }

        public virtual void VisitEmpty(EmptySyntax syntax)
        {
        }

        public virtual void VisitExpressionStatement(ExpressionStatementSyntax syntax)
        {
            syntax.Expression.Accept(this);
        }

        public virtual void VisitForEachIn(ForEachInSyntax syntax)
        {
            syntax.Initialization.Accept(this);
            syntax.Expression.Accept(this);
            syntax.Body.Accept(this);
        }

        public virtual void VisitFor(ForSyntax syntax)
        {
            if (syntax.Initialization != null)
                syntax.Initialization.Accept(this);
            if (syntax.Test != null)
                syntax.Test.Accept(this);
            if (syntax.Increment != null)
                syntax.Increment.Accept(this);
            syntax.Body.Accept(this);
        }

        public virtual void VisitFunctionDeclaration(FunctionDeclarationSyntax syntax)
        {
            if (syntax.Body != null)
                syntax.Body.Accept(this);
        }

        public virtual void VisitIf(IfSyntax syntax)
        {
            syntax.Test.Accept(this);
            syntax.Then.Accept(this);
            if (syntax.Else != null)
                syntax.Else.Accept(this);
        }

        public virtual void VisitReturn(ReturnSyntax syntax)
        {
            if (syntax.Expression != null)
                syntax.Expression.Accept(this);
        }

        public virtual void VisitSwitch(SwitchSyntax syntax)
        {
            syntax.Expression.Accept(this);
            foreach (var @case in syntax.Cases)
            {
                @case.Expression.Accept(this);
                if (@case.Body != null)
                    @case.Body.Accept(this);
            }
            if (syntax.Default != null)
                syntax.Default.Accept(this);
        }

        public virtual void VisitWith(WithSyntax syntax)
        {
            syntax.Expression.Accept(this);
            syntax.Body.Accept(this);
        }

        public virtual void VisitThrow(ThrowSyntax syntax)
        {
            if (syntax.Expression != null)
                syntax.Expression.Accept(this);
        }

        public virtual void VisitTry(TrySyntax syntax)
        {
            syntax.Body.Accept(this);
            if (syntax.Catch != null)
                syntax.Catch.Body.Accept(this);
            if (syntax.Finally != null)
                syntax.Finally.Body.Accept(this);
        }

        public virtual void VisitVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            if (syntax.Expression != null)
                syntax.Expression.Accept(this);
        }

        public virtual void VisitWhile(WhileSyntax syntax)
        {
            syntax.Test.Accept(this);
            syntax.Body.Accept(this);
        }

        public virtual void VisitArrayDeclaration(ArrayDeclarationSyntax syntax)
        {
            foreach (var parameter in syntax.Parameters)
            {
                parameter.Accept(this);
            }
        }

        public virtual void VisitCommaOperator(CommaOperatorSyntax syntax)
        {
            foreach (var expression in syntax.Expressions)
            {
                expression.Accept(this);
            }
        }

        public virtual void VisitFunction(FunctionSyntax syntax)
        {
            syntax.Body.Accept(this);
        }

        public virtual void VisitMethodCall(MethodCallSyntax syntax)
        {
            syntax.Expression.Accept(this);
            foreach (var argument in syntax.Arguments)
            {
                argument.Expression.Accept(this);
            }
            foreach (var generic in syntax.Generics)
            {
                generic.Accept(this);
            }
        }

        public virtual void VisitIndexer(IndexerSyntax syntax)
        {
            syntax.Expression.Accept(this);
            syntax.Index.Accept(this);
        }

        public virtual void VisitProperty(PropertySyntax syntax)
        {
            syntax.Expression.Accept(this);
        }

        public virtual void VisitPropertyDeclaration(PropertyDeclarationSyntax syntax)
        {
            if (syntax.Expression != null)
                syntax.Expression.Accept(this);
        }

        public virtual void VisitIdentifier(IdentifierSyntax syntax)
        {
        }

        public virtual void VisitJsonExpression(JsonExpressionSyntax syntax)
        {
            foreach (var property in syntax.Properties)
            {
                var dataProperty = property as JsonDataProperty;
                if (dataProperty != null)
                {
                    dataProperty.Expression.Accept(this);
                }
                else
                {
                    var accessorProperty = (JsonAccessorProperty)property;
                    if (accessorProperty.GetExpression != null)
                        accessorProperty.GetExpression.Accept(this);
                    if (accessorProperty.SetExpression != null)
                        accessorProperty.SetExpression.Accept(this);
                }
            }
        }

        public virtual void VisitNew(NewSyntax syntax)
        {
            syntax.Expression.Accept(this);
        }

        public virtual void VisitBinaryExpression(BinaryExpressionSyntax syntax)
        {
            syntax.Left.Accept(this);
            syntax.Right.Accept(this);
        }

        public virtual void VisitTernary(TernarySyntax syntax)
        {
            syntax.Test.Accept(this);
            syntax.Then.Accept(this);
            syntax.Else.Accept(this);
        }

        public virtual void VisitUnaryExpression(UnaryExpressionSyntax syntax)
        {
            syntax.Operand.Accept(this);
        }

        public virtual void VisitValue(ValueSyntax syntax)
        {
        }

        public virtual void VisitRegexp(RegexpSyntax syntax)
        {
        }

        public virtual void VisitClrIdentifier(ClrIdentifierSyntax syntax)
        {
        }

        public virtual void VisitLabel(LabelSyntax syntax)
        {
            syntax.Expression.Accept(this);
        }
    }
}
