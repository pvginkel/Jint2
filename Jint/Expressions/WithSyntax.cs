using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    public class WithSyntax : SyntaxNode, ISourceLocation
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.With; }
        }

        public ExpressionSyntax Expression { get; private set; }
        public SyntaxNode Body { get; private set; }
        internal Variable Target { get; set; }
        public SourceLocation Location { get; private set; }

        public WithSyntax(ExpressionSyntax expression, SyntaxNode body, SourceLocation location)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (body == null)
                throw new ArgumentNullException("body");
            if (location == null)
                throw new ArgumentNullException("location");

            Expression = expression;
            Body = body;
            Location = location;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitWith(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitWith(this);
        }
    }
}
