using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class BinaryExpressionSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Binary; }
        }

        public ExpressionSyntax Left { get; private set; }
        public ExpressionSyntax Right { get; private set; }
        public SyntaxExpressionType Operation { get; private set; }

        public BinaryExpressionSyntax(SyntaxExpressionType operation, ExpressionSyntax left, ExpressionSyntax right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");

            Operation = operation;
            Left = left;
            Right = right;
        }

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
