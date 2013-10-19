using System;

namespace Jint.Expressions {
    [Serializable]
    public class ValueExpression : Expression {
        public ValueExpression(object value, TypeCode typeCode) {
            Value = value;
            TypeCode = typeCode;
        }

        public object Value { get; set; }

        public TypeCode TypeCode { get; set; }

        [System.Diagnostics.DebuggerStepThrough]
        public override void Accept(IStatementVisitor visitor) {
            visitor.Visit(this);
        }

        public override string ToString() {
            return Value.ToString();
        }
    }
}
