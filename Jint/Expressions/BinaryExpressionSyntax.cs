using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax(BinaryExpressionType operation, ExpressionSyntax leftExpression, ExpressionSyntax rightExpression)
        {
            Operation = operation;
            Left = leftExpression;
            Right = rightExpression;
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.Binary; }
        }

        public ExpressionSyntax Left { get; set; }

        public ExpressionSyntax Right { get; set; }

        public BinaryExpressionType Operation { get; set; }

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
            return Operation + " (" + Left + ", " + Right + ")";
        }
    }
}
