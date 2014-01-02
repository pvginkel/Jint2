using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
{
    internal class NewSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.New; }
        }

        // We specifically do not allow something like new Array to be a literal.
        // The reason for this is that Array can be redefined; [] cannot.

        public ExpressionSyntax Expression { get; private set; }

        public NewSyntax(ExpressionSyntax expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Expression = expression;
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitNew(this);
        }
    }
}
