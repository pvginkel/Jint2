using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class ValueSyntax : ExpressionSyntax
    {
        public ValueSyntax(object value, TypeCode typeCode)
        {
            Value = value;
            TypeCode = typeCode;
        }

        public object Value { get; set; }

        public TypeCode TypeCode { get; set; }

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
