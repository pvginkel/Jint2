using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    internal class ForSyntax : SyntaxNode, IForStatement, ISourceLocation
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
