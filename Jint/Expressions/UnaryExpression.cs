using System;

namespace Jint.Expressions {
    [Serializable]
    public class UnaryExpression : Expression {
        public UnaryExpression(UnaryExpressionType type, Expression expression) {
            Type = type;
            Expression = expression;
        }

        public Expression Expression { get; set; }

        public UnaryExpressionType Type { get; set; }

        [System.Diagnostics.DebuggerStepThrough]
        public override void Accept(IStatementVisitor visitor) {
            visitor.Visit(this);
        }
    }

    public enum UnaryExpressionType {
        TypeOf,
        New,
        Not,
        Negate,
        Positive,
        PrefixPlusPlus,
        PrefixMinusMinus,
        PostfixPlusPlus,
        PostfixMinusMinus,
        Delete,
        Void,
        Inv,
        Unknown
    }
}
