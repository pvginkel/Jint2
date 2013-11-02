using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class ThrowSyntax : SyntaxNode
    {
        public ExpressionSyntax Expression { get; set; }

        public ThrowSyntax(ExpressionSyntax expression)
        {
            Expression = expression;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitThrow(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitThrow(this);
        }
    }
}
