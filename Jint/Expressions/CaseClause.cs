using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions
{
    public class CaseClause
    {
        public ExpressionSyntax Expression { get; private set; }
        public BlockSyntax Body { get; private set; }

        public CaseClause(ExpressionSyntax expression, BlockSyntax body)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (body == null)
                throw new ArgumentNullException("body");

            Expression = expression;
            Body = body;
        }
    }
}
