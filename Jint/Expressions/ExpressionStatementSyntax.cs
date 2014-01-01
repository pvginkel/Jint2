using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    internal class ExpressionStatementSyntax : SyntaxNode, ISourceLocation
    {
        public ExpressionSyntax Expression { get; private set; }
        public SourceLocation Location { get; private set; }

        public override SyntaxType Type
        {
            get { return SyntaxType.ExpressionStatement; }
        }

        public ExpressionStatementSyntax(ExpressionSyntax expression, SourceLocation location)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (location == null)
                throw new ArgumentNullException("location");

            Expression = expression;
            Location = location;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitExpressionStatement(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitExpressionStatement(this);
        }
    }
}
