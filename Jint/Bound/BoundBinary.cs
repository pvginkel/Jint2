using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundBinary : BoundExpression
    {
        public BoundExpressionType Operation { get; set; }
        public BoundExpression Left { get; set; }
        public BoundExpression Right { get; set; }

        public override BoundNodeType NodeType
        {
            get { return BoundNodeType.Binary; }
        }

        public BoundBinary(BoundExpressionType operation, BoundExpression left, BoundExpression right)
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
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitBinary(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitBinary(this);
        }

        public BoundBinary Update(BoundExpressionType operation, BoundExpression left, BoundExpression right)
        {
            if (
                operation == Operation &&
                left == Left &&
                right == Right
            )
                return this;

            return new BoundBinary(operation, left, right);
        }
    }
}
