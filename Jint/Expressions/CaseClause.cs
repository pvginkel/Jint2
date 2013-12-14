using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions
{
    internal class CaseClause : ISourceLocation
    {
        public ExpressionSyntax Expression { get; private set; }
        public BlockSyntax Body { get; private set; }
        public SourceLocation Location { get; private set; }

        public CaseClause(ExpressionSyntax expression, BlockSyntax body, SourceLocation location)
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
    }
}
