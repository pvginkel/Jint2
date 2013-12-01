using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class ExpressionStatementSyntax : SyntaxNode
    {
        public ExpressionSyntax Expression { get; private set; }

        internal override bool IsLiteral
        {
            get { return Expression.IsLiteral; }
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.ExpressionStatement; }
        }

        public ExpressionStatementSyntax(ExpressionSyntax expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Expression = expression;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitExpressionStatement(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitExpressionStatement(this);
        }
    }
}
