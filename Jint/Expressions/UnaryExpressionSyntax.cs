using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class UnaryExpressionSyntax : ExpressionSyntax
    {
        public UnaryExpressionSyntax(SyntaxExpressionType operation, ExpressionSyntax operand)
        {
            Operation = operation;
            Operand = operand;
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.Unary; }
        }

        public ExpressionSyntax Operand { get; set; }

        public SyntaxExpressionType Operation { get; set; }

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
