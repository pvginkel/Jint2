using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
{
    internal class WhileSyntax : SyntaxNode, ISourceLocation
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
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitWhile(this);
        }
    }
}
