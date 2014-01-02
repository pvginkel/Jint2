using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
{
    internal class BreakSyntax : SyntaxNode, ISourceLocation
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
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitBreak(this);
        }
    }
}
