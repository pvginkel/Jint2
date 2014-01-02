using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
{
    internal class ContinueSyntax : SyntaxNode, ISourceLocation
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
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitContinue(this);
        }
    }
}
