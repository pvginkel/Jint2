using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class IfSyntax : SyntaxNode
    {
        public ExpressionSyntax Test { get; set; }
        public SyntaxNode Then { get; set; }
        public SyntaxNode Else { get; set; }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitIf(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitIf(this);
        }
    }
}
