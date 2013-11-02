using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class UnaryExpressionSyntax : ExpressionSyntax
    {
        public UnaryExpressionSyntax(UnaryExpressionType type, ExpressionSyntax operand)
        {
            Type = type;
            Operand = operand;
        }

        public ExpressionSyntax Operand { get; set; }

        public UnaryExpressionType Type { get; set; }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitUnaryExpression(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }
    }
}
