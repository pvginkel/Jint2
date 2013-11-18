using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class FinallyClause
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
