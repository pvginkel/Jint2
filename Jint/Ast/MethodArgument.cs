using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Ast
{
    internal class MethodArgument
    {
        public ExpressionSyntax Expression { get; private set; }
        public bool IsRef { get; private set; }

        public MethodArgument(ExpressionSyntax expression, bool isRef)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Expression = expression;
            IsRef = isRef;
        }
    }
}
