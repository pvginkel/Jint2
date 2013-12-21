using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundSetMember : BoundStatement
    {
        public BoundExpression Expression { get; private set; }
        public BoundExpression Index { get; private set; }
        public BoundExpression Value { get; private set; }

        public override BoundNodeType NodeType
        {
            get { return BoundNodeType.SetMember; }
        }

        public BoundSetMember(BoundExpression expression, BoundExpression index, BoundExpression value)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (index == null)
                throw new ArgumentNullException("index");
            if (value == null)
                throw new ArgumentNullException("value");

            Expression = expression;
            Index = index;
            Value = value;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitSetMember(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitSetMember(this);
        }

        public BoundSetMember Update(BoundExpression expression, BoundExpression index, BoundExpression value)
        {
            if (
                expression == Expression &&
                index == Index &&
                value == Value
            )
                return this;

            return new BoundSetMember(expression, index, value);
        }
    }
}
