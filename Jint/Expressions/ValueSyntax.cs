using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    internal class ValueSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Value; }
        }

        public object Value { get; private set; }

        public ValueSyntax(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            Value = value;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitValue(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitValue(this);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
