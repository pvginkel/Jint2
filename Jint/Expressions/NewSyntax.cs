using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class NewSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.New; }
        }

        public ExpressionSyntax Expression { get; set; }

        public NewSyntax(ExpressionSyntax expression)
        {
            Expression = expression;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitNew(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitNew(this);
        }
    }
}
