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

        public ExpressionSyntax Expression { get; set; }
        public SyntaxNode Body { get; set; }
        public Variable Target { get; set; }

        public WithSyntax(ExpressionSyntax expression, SyntaxNode statement)
        {
            Body = statement;
            Expression = expression;
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
