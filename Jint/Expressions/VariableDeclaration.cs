using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal class VariableDeclaration
    {
        public string Identifier { get; private set; }
        public ExpressionSyntax Expression { get; private set; }
        public bool Global { get; private set; }
        public Variable Target { get; private set; }

        public VariableDeclaration(string identifier, ExpressionSyntax expression, bool global, Variable target)
        {
            if (identifier == null)
                throw new ArgumentNullException("identifier");
            if (target == null)
                throw new ArgumentNullException("target");

            Identifier = identifier;
            Expression = expression;
            Global = global;
            Target = target;
        }
    }
}
