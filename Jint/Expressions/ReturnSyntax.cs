using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    public class ReturnSyntax : SyntaxNode
    {
        public ExpressionSyntax Expression { get; private set; }

        public ReturnSyntax(ExpressionSyntax expression)
        {
            Expression = expression;
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.Return; }
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitReturn(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitReturn(this);
        }
    }
}
