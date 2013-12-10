using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Jint.Compiler;

namespace Jint.Expressions
{
    public class WhileSyntax : SyntaxNode, ISourceLocation
    {
        public ExpressionSyntax Test { get; private set; }
        public SyntaxNode Body { get; private set; }
        public SourceLocation Location { get; private set; }

        public WhileSyntax(ExpressionSyntax test, SyntaxNode body, SourceLocation location)
        {
            if (test == null)
                throw new ArgumentNullException("test");
            if (body == null)
                throw new ArgumentNullException("body");
            if (location == null)
                throw new ArgumentNullException("location");

            Test = test;
            Body = body;
            Location = location;
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.While; }
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitWhile(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitWhile(this);
        }
    }
}
