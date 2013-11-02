using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class CommaOperatorSyntax : ExpressionSyntax
    {
        public List<SyntaxNode> Expressions { get; set; }

        public CommaOperatorSyntax()
        {
            Expressions = new List<SyntaxNode>();
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitCommaOperator(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommaOperator(this);
        }
    }
}
