using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundUnary : BoundExpression
    {
        public BoundExpressionType Operation { get; private set; }
        public BoundExpression Operand { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.Unary; }
        }

        public override BoundValueType ValueType
        {
            get { return ResolveValueType(); }
        }

        private BoundValueType ResolveValueType()
        {
            switch (Operation)
            {
                case BoundExpressionType.BitwiseNot:
                case BoundExpressionType.Negate:
                case BoundExpressionType.UnaryPlus:
                    return BoundValueType.Number;

                case BoundExpressionType.Not:
                    return BoundValueType.Boolean;

                case BoundExpressionType.TypeOf:
                    return BoundValueType.String;

                case BoundExpressionType.Void:
                    return BoundValueType.Unknown;

                default:
                    throw new InvalidOperationException();
            }
        }

        public BoundUnary(BoundExpressionType operation, BoundExpression operand)
        {
            if (operand == null)
                throw new ArgumentNullException("operand");

            Operation = operation;
            Operand = operand;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitUnary(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitUnary(this);
        }

        public BoundUnary Update(BoundExpressionType operation, BoundExpression operand)
        {
            if (
                operation == Operation &&
                operand == Operand
            )
                return this;

            return new BoundUnary(operation, operand);
        }
    }
}
