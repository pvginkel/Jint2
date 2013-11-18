using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class UnaryExpressionSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Unary; }
        }

        public ExpressionSyntax Operand { get; private set; }
        public SyntaxExpressionType Operation { get; private set; }

        public UnaryExpressionSyntax(SyntaxExpressionType operation, ExpressionSyntax operand)
        {
            if (operand == null)
                throw new ArgumentNullException("operand");

            Operation = operation;
            Operand = operand;
        }

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
