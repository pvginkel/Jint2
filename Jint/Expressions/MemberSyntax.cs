using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal abstract class MemberSyntax : ExpressionSyntax
    {
        public ExpressionSyntax Expression { get; private set; }
        public override bool IsAssignable { get { return true; } }

        protected MemberSyntax(ExpressionSyntax expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Expression = expression;
        }
    }
}
