using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    public class BreakSyntax : SyntaxNode, ISourceLocation
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Break; }
        }

        public string Target { get; private set; }
        public SourceLocation Location { get; private set; }

        public BreakSyntax(string target, SourceLocation location)
        {
            if (location == null)
                throw new ArgumentNullException("location");

            Target = target;
            Location = location;
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
