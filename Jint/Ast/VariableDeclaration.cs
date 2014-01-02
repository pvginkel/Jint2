using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Ast
{
    internal class VariableDeclaration
    {
        public IIdentifier Identifier { get; private set; }
        public ExpressionSyntax Expression { get; private set; }
        public bool Global { get; private set; }

        public VariableDeclaration(IIdentifier identifier, ExpressionSyntax expression, bool global)
        {
            if (identifier == null)
                throw new ArgumentNullException("identifier");

            Identifier = identifier;
            Expression = expression;
            Global = global;
        }
    }
}
