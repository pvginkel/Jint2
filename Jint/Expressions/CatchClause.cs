using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions
{
    internal class CatchClause
    {
        public string Identifier { get; private set; }
        public SyntaxNode Body { get; private set; }
        public Variable Target { get; private set; }

        public CatchClause(string identifier, SyntaxNode statement, Variable target)
        {
            if (identifier == null)
                throw new ArgumentNullException("identifier");
            if (statement == null)
                throw new ArgumentNullException("statement");
            if (target == null)
                throw new ArgumentNullException("target");

            Identifier = identifier;
            Body = statement;
            Target = target;
        }
    }
}
