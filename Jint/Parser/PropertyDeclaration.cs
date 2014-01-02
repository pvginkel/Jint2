using System;
using System.Collections.Generic;
using System.Text;
using Jint.Ast;

namespace Jint.Parser
{
    internal class PropertyDeclaration
    {
        public string Name { get; private set; }
        public ExpressionSyntax Expression { get; private set; }
        public PropertyExpressionType Mode { get; private set; }

        public PropertyDeclaration(string name, ExpressionSyntax expression, PropertyExpressionType mode)
        {
            Name = name;
            Expression = expression;
            Mode = mode;
        }
    }
}
