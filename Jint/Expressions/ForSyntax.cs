using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class ForSyntax : SyntaxNode, IForStatement
    {
        public SyntaxNode Initialization { get; set; }
        public SyntaxNode Test { get; set; }
        public SyntaxNode Increment { get; set; }
        public SyntaxNode Body { get; set; }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitFor(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitFor(this);
        }
    }
}
