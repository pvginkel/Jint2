using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    public class ThrowSyntax : SyntaxNode, ISourceLocation
    {
        public ExpressionSyntax Expression { get; private set; }
        public SourceLocation Location { get; private set; }

        public ThrowSyntax(ExpressionSyntax expression, SourceLocation location)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (location == null)
                throw new ArgumentNullException("location");

            Expression = expression;
            Location = location;
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.Throw; }
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitThrow(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitThrow(this);
        }
    }
}
