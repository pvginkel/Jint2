using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Ast
{
    internal class CatchClause
    {
        public IIdentifier Identifier { get; private set; }
        public SyntaxNode Body { get; private set; }

        public CatchClause(IIdentifier identifier, SyntaxNode statement)
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
