using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Ast
{
    internal class SwitchCase : ISourceLocation
    {
        public ExpressionSyntax Expression { get; private set; }
        public BlockSyntax Body { get; private set; }
        public SourceLocation Location { get; private set; }

        public SwitchCase(ExpressionSyntax expression, BlockSyntax body, SourceLocation location)
        {
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
