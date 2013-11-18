using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class CatchClause
    {
        public string Identifier { get; private set; }
        public SyntaxNode Body { get; private set; }
        internal Variable Target { get; set; }

        public CatchClause(string identifier, SyntaxNode statement)
        {
            if (identifier == null)
                throw new ArgumentNullException("identifier");
            if (statement == null)
                throw new ArgumentNullException("statement");

            Identifier = identifier;
            Body = statement;
        }
    }
}
