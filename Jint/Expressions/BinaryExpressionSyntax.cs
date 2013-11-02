using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax(BinaryExpressionType type, ExpressionSyntax leftExpression, ExpressionSyntax rightExpression)
        {
            Type = type;
            Left = leftExpression;
            Right = rightExpression;
        }

        public ExpressionSyntax Left { get; set; }

        public ExpressionSyntax Right { get; set; }

        public BinaryExpressionType Type { get; set; }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitBinaryExpression(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }

        public override string ToString()
        {
            return Type + " (" + Left + ", " + Right + ")";
        }
    }
}
