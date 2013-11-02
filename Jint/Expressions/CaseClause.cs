using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class CaseClause
    {
        public ExpressionSyntax Expression { get; set; }
        public BlockSyntax Body { get; private set; }

        public CaseClause()
        {
            Body = new BlockSyntax();
        }
    }
}
