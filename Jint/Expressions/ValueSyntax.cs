using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class ValueSyntax : ExpressionSyntax
    {
        private readonly ValueType _valueType;

        public override SyntaxType Type
        {
            get { return SyntaxType.Value; }
        }

        public object Value { get; private set; }

        internal override ValueType ValueType
        {
            get { return _valueType; }
        }

        public ValueSyntax(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            Value = value;
            _valueType = SyntaxUtil.GetValueType(value.GetType());
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
