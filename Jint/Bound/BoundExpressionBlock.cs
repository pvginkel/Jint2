using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundExpressionBlock : BoundExpression
    {
        public IBoundReadable Result { get; private set; }
        public BoundBlock Body { get; private set; }

        public override BoundNodeType NodeType
        {
            get { return BoundNodeType.ExpressionBlock; }
        }

        public BoundExpressionBlock(IBoundReadable result, BoundBlock body)
        {
            if (result == null)
                throw new ArgumentNullException("result");
            if (body == null)
                throw new ArgumentNullException("body");

            Result = result;
            Body = body;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitExpressionBlock(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitExpressionBlock(this);
        }

        public BoundExpressionBlock Update(IBoundReadable result, BoundBlock body)
        {
            if (
                result == Result &&
                body == Body
            )
                return this;

            return new BoundExpressionBlock(result, body);
        }
    }
}
