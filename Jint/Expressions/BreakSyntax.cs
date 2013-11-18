using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class BreakSyntax : SyntaxNode
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Break; }
        }

        public string Target { get; private set; }

        public BreakSyntax(string target)
        {
            Target = target;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitBreak(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitBreak(this);
        }
    }
}
