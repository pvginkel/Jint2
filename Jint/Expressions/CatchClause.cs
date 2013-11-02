using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class CatchClause
    {
        public string Identifier { get; set; }
        public SyntaxNode Body { get; set; }

        public CatchClause(string identifier, SyntaxNode statement)
        {
            Identifier = identifier;
            Body = statement;
        }
    }
}
