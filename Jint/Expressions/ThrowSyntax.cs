using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class ThrowSyntax : SyntaxNode
    {
        public ExpressionSyntax Expression { get; private set; }

        public ThrowSyntax(ExpressionSyntax expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Expression = expression;
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.Throw; }
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
