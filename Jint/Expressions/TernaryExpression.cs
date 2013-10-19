using System;

namespace Jint.Expressions {
    [Serializable]
    public class TernaryExpression : Expression {
        public TernaryExpression(Expression leftExpression, Expression middleExpression, Expression rightExpression) {
            LeftExpression = leftExpression;
            MiddleExpression = middleExpression;
            RightExpression = rightExpression;
        }

        public Expression LeftExpression { get; set; }

        public Expression MiddleExpression { get; set; }

        public Expression RightExpression { get; set; }

        [System.Diagnostics.DebuggerStepThrough]
        public override void Accept(IStatementVisitor visitor) {
            visitor.Visit(this);
        }

        public override string ToString() {
            return LeftExpression + " (" + MiddleExpression + ", " + RightExpression + ")";
        }
    }

}
