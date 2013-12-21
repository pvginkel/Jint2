using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundNew : BoundExpression
    {
        public BoundExpression Expression { get; private set; }

        public override BoundNodeType NodeType
        {
            get { return BoundNodeType.New; }
        }

        public BoundNew(BoundExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Expression = expression;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitNew(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitNew(this);
        }

        public BoundNew Update(BoundExpression expression)
        {
            if (expression == Expression)
                return this;

            return new BoundNew(expression);
        }
    }
}
