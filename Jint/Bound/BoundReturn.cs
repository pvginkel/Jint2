using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundReturn : BoundStatement
    {
        public BoundExpression Expression { get; private set; }

        public override BoundNodeType NodeType
        {
            get { return BoundNodeType.Return; }
        }

        public BoundReturn(BoundExpression expression)
        {
            Expression = expression;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitReturn(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitReturn(this);
        }

        public BoundReturn Update(BoundExpression expression)
        {
            if (expression == Expression)
                return this;

            return new BoundReturn(expression);
        }
    }
}
