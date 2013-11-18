using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class WhileSyntax : SyntaxNode
    {
        public ExpressionSyntax Test { get; private set; }
        public SyntaxNode Body { get; private set; }

        public WhileSyntax(ExpressionSyntax test, SyntaxNode body)
        {
            if (test == null)
                throw new ArgumentNullException("test");
            if (body == null)
                throw new ArgumentNullException("body");

            Test = test;
            Body = body;
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
