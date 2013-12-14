using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    public class VariableDeclaration
    {
        public string Identifier { get; private set; }
        public ExpressionSyntax Expression { get; private set; }
        public bool Global { get; private set; }
        internal Variable Target { get; set; }

        public VariableDeclaration(string identifier, ExpressionSyntax expression, bool global)
        {
            if (identifier == null)
                throw new ArgumentNullException("identifier");

            Identifier = identifier;
            Expression = expression;
            Global = global;
        }
    }
}
