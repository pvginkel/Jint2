using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundCallArgument : BoundNode
    {
        public BoundExpression Expression { get; private set; }
        public bool IsRef { get; private set; }

        public override BoundNodeType NodeType
        {
            get { return BoundNodeType.CallArgument; }
        }

        public BoundCallArgument(BoundExpression expression, bool isRef)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Expression = expression;
            IsRef = isRef;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitCallArgument(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitCallArgument(this);
        }

        public BoundCallArgument Update(BoundExpression expression, bool isRef)
        {
            if (
                expression == Expression &&
                isRef == IsRef
            )
                return this;

            return new BoundCallArgument(expression, isRef);
        }
    }
}
