using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
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
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitValue(this);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
