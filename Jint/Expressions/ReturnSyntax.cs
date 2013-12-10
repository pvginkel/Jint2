using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Jint.Compiler;

namespace Jint.Expressions
{
    public class ReturnSyntax : SyntaxNode, ISourceLocation
    {
        public ExpressionSyntax Expression { get; private set; }
        public SourceLocation Location { get; private set; }

        public ReturnSyntax(ExpressionSyntax expression, SourceLocation location)
        {
            if (location == null)
                throw new ArgumentNullException("location");

            Expression = expression;
            Location = location;
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.Return; }
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitReturn(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitReturn(this);
        }
    }
}
