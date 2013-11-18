using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class ContinueSyntax : SyntaxNode
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Continue; }
        }

        public string Target { get; private set; }

        public ContinueSyntax(string target)
        {
            Target = target;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitContinue(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitContinue(this);
        }
    }
}
