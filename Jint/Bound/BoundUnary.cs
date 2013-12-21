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

        public override BoundNodeType NodeType
        {
            get { return BoundNodeType.Unary; }
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
