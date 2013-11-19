using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class NewSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.New; }
        }

        public ExpressionSyntax Expression { get; private set; }

        internal override ValueType ValueType
        {
            get { return ValueType.Unknown; }
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
