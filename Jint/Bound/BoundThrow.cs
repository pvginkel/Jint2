using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundThrow : BoundStatement
    {
        public BoundExpression Expression { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.Throw; }
        }

        public BoundThrow(BoundExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Expression = expression;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitThrow(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitThrow(this);
        }

        public BoundThrow Update(BoundExpression expression)
        {
            if (expression == Expression)
                return this;

            return new BoundThrow(expression);
        }
    }
}
