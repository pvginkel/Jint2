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

        public override BoundKind Kind
        {
            get { return BoundKind.Binary; }
        }

        public override BoundValueType ValueType
        {
            get { return ResolveValueType(); }
        }

        private BoundValueType ResolveValueType()
        {
            var left = Left.ValueType;
            var right = Right.ValueType;

            switch (Operation)
            {
                case BoundExpressionType.Add:
                    if (
                        left == BoundValueType.Unknown || left == BoundValueType.String ||
                        right == BoundValueType.Unknown || right == BoundValueType.String
                    )
                        return BoundValueType.Unknown;

                    return BoundValueType.Number;

                case BoundExpressionType.BitwiseAnd:
                case BoundExpressionType.BitwiseExclusiveOr:
                case BoundExpressionType.BitwiseOr:
                case BoundExpressionType.Divide:
                case BoundExpressionType.LeftShift:
                case BoundExpressionType.RightShift:
                case BoundExpressionType.UnsignedRightShift:
                case BoundExpressionType.Modulo:
                case BoundExpressionType.Multiply:
                case BoundExpressionType.Subtract:
                    return BoundValueType.Number;

                case BoundExpressionType.Equal:
                case BoundExpressionType.NotEqual:
                case BoundExpressionType.Same:
                case BoundExpressionType.NotSame:
                case BoundExpressionType.LessThan:
                case BoundExpressionType.LessThanOrEqual:
                case BoundExpressionType.GreaterThan:
                case BoundExpressionType.GreaterThanOrEqual:
                case BoundExpressionType.In:
                case BoundExpressionType.InstanceOf:
                    return BoundValueType.Boolean;

                default:
                    throw new InvalidOperationException();
            }
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
