using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Ast
{
    internal class FinallyClause
    {
        public SyntaxNode Body { get; private set; }

        public FinallyClause(SyntaxNode body)
        {
            if (body == null)
                throw new ArgumentNullException("body");

            Body = body;
        }
    }
}
