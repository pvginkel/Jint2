using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
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

        public override ValueType ValueType
        {
            get { return ValueType.Object; }
        }

        public NewSyntax(ExpressionSyntax expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Expression = expression;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitNew(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitNew(this);
        }
    }
}
