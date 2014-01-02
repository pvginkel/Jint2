using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
{
    internal class ForSyntax : SyntaxNode, ISourceLocation
    {
        public SyntaxNode Initialization { get; private set; }
        public SyntaxNode Test { get; private set; }
        public SyntaxNode Increment { get; private set; }
        public SyntaxNode Body { get; private set; }
        public SourceLocation Location { get; private set; }

        public override SyntaxType Type
        {
            get { return SyntaxType.For; }
        }

        public ForSyntax(SyntaxNode initialization, SyntaxNode test, SyntaxNode increment, SyntaxNode body, SourceLocation location)
        {
            if (body == null)
                throw new ArgumentNullException("body");
            if (location == null)
                throw new ArgumentNullException("location");

            Initialization = initialization;
            Test = test;
            Increment = increment;
            Body = body;
            Location = location;
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitFor(this);
        }
    }
}
