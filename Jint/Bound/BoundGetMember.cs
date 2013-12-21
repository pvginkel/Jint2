using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundGetMember : BoundExpression
    {
        public BoundExpression Expression { get; private set; }
        public BoundExpression Index { get; private set; }

        public override BoundNodeType NodeType
        {
            get { return BoundNodeType.GetMember; }
        }

        public BoundGetMember(BoundExpression expression, BoundExpression index)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (index == null)
                throw new ArgumentNullException("index");

            Expression = expression;
            Index = index;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitGetMember(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitGetMember(this);
        }

        public BoundGetMember Update(BoundExpression expression, BoundExpression index)
        {
            if (
                expression == Expression &&
                index == Index
            )
                return this;

            return new BoundGetMember(expression, index);
        }
    }
}
