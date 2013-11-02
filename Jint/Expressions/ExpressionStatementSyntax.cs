using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class ExpressionStatementSyntax : SyntaxNode
    {
        public ExpressionSyntax Expression { get; set; }

        public ExpressionStatementSyntax(ExpressionSyntax expression)
        {
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
