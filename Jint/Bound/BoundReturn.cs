using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Expressions;

namespace Jint.Bound
{
    internal class BoundReturn : BoundStatement
    {
        public BoundExpression Expression { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.Return; }
        }

        public BoundReturn(BoundExpression expression, SourceLocation location)
            : base(location)
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

        public BoundReturn Update(BoundExpression expression, SourceLocation location)
        {
            if (
                expression == Expression &&
                location == Location
            )
                return this;

            return new BoundReturn(expression, location);
        }
    }
}
