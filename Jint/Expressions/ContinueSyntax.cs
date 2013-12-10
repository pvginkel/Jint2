using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Jint.Compiler;

namespace Jint.Expressions
{
    public class ContinueSyntax : SyntaxNode, ISourceLocation
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Continue; }
        }

        public string Target { get; private set; }
        public SourceLocation Location { get; private set; }

        public ContinueSyntax(string target, SourceLocation location)
        {
            if (location == null)
                throw new ArgumentNullException("location");

            Location = location;
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
