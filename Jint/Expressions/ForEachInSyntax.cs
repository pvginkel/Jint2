using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class ForEachInSyntax : SyntaxNode, IForStatement
    {
        public SyntaxNode Initialization { get; set; }
        public ExpressionSyntax Expression { get; set; }
        public SyntaxNode Body { get; set; }

        public override SyntaxType Type
        {
            get { return SyntaxType.ForEachIn; }
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitForEachIn(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitForEachIn(this);
        }
    }
}
