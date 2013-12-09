using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Expressions;
using ValueType = Jint.Expressions.ValueType;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private LiteralVisitor _literalVisitor;

        internal object BuildLiteral(SyntaxNode syntaxNode)
        {
            if (_literalVisitor == null)
                _literalVisitor = new LiteralVisitor(this);

            return syntaxNode.Accept(_literalVisitor);
        }

        private class LiteralVisitor : SyntaxVisitor<object>
        {
            private readonly JsGlobal _global;

            public LiteralVisitor(JsGlobal global)
            {
                _global = global;
            }

            public override object VisitCommaOperator(CommaOperatorSyntax syntax)
            {
                Debug.Assert(syntax.IsLiteral);

                return syntax.Expressions[0].Accept(this);
            }

            public override object VisitExpressionStatement(ExpressionStatementSyntax syntax)
            {
                return syntax.Expression.Accept(this);
            }

            public override object VisitValue(ValueSyntax syntax)
            {
                switch (syntax.ValueType)
                {
                    case ValueType.Boolean: return BooleanBoxes.Box((bool)syntax.Value);
                    case ValueType.Double: return (double)syntax.Value;
                    case ValueType.String: return (string)syntax.Value;
                    default: throw new InvalidOperationException();
                }
            }

            public override object VisitArrayDeclaration(ArrayDeclarationSyntax syntax)
            {
                var array = _global.CreateArray();

                for (int i = 0; i < syntax.Parameters.Count; i++)
                {
                    array.SetProperty(i, syntax.Parameters[i].Accept(this));
                }

                return array;
            }

            public override object VisitJsonExpression(JsonExpressionSyntax syntax)
            {
                var @object = _global.CreateObject();

                foreach (JsonDataProperty property in syntax.Properties)
                {
                    @object.SetProperty(property.Name, property.Expression.Accept(this));
                }

                return @object;
            }

            public override object VisitProgram(ProgramSyntax syntax)
            {
                // The IsLiteral of program ensures that we either have a
                // single statement that is a literal, or we have a single
                // return that is a literal.

                foreach (var statement in syntax.Statements)
                {
                    if (statement.Type == SyntaxType.Return)
                        return ((ReturnSyntax)statement).Expression.Accept(this);
                    if (statement.IsLiteral)
                        return statement.Accept(this);
                }

                // If IsLiteral of ProgramExpression did its work, we should never
                // get here.

                throw new InvalidOperationException();
            }
        }
    }
}
