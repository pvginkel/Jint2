using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class ValueSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Value; }
        }

        public object Value { get; private set; }
        public TypeCode TypeCode { get; private set; }

        public ValueSyntax(object value, TypeCode typeCode)
        {
            Value = value;
            TypeCode = typeCode;
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
