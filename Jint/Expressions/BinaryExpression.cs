using System;

namespace Jint.Expressions {
    [Serializable]
    public class BinaryExpression : Expression {
        public BinaryExpression(BinaryExpressionType type, Expression leftExpression, Expression rightExpression) {
            Type = type;
            LeftExpression = leftExpression;
            RightExpression = rightExpression;
        }

        public Expression LeftExpression { get; set; }

        public Expression RightExpression { get; set; }

        public BinaryExpressionType Type { get; set; }

        [System.Diagnostics.DebuggerStepThrough]
        public override void Accept(IStatementVisitor visitor) {
            visitor.Visit(this);
        }

        public override string ToString() {
            return Type + " (" + LeftExpression + ", " + RightExpression + ")";
        }
    }

    public enum BinaryExpressionType {
        And,
        Or,
        NotEqual,
        LesserOrEqual,
        GreaterOrEqual,
        Lesser,
        Greater,
        Equal,
        Minus,
        Plus,
        Modulo,
        Div,
        Times,
        Pow,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXOr,
        Same,
        NotSame,
        LeftShift,
        RightShift,
        UnsignedRightShift,
        InstanceOf,
        In,
        Unknown
    }


}
