using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class FinallyClause
    {
        public SyntaxNode Body { get; set; }

        public FinallyClause(SyntaxNode statement)
        {
            Body = statement;
        }
    }
}
