using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class WithSyntax : SyntaxNode
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.With; }
        }

        public ExpressionSyntax Expression { get; private set; }
        public SyntaxNode Body { get; private set; }
        internal Variable Target { get; set; }

        public WithSyntax(ExpressionSyntax expression, SyntaxNode body)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (body == null)
                throw new ArgumentNullException("body");

            Expression = expression;
            Body = body;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitWith(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitWith(this);
        }
    }
}
