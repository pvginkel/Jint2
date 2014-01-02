using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Ast;

namespace Jint.Bound
{
    internal class BoundThrow : BoundStatement
    {
        public BoundExpression Expression { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.Throw; }
        }

        public BoundThrow(BoundExpression expression, SourceLocation location)
            : base(location)
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

        public BoundThrow Update(BoundExpression expression, SourceLocation location)
        {
            if (
                expression == Expression &&
                location == Location
            )
                return this;

            return new BoundThrow(expression, location);
        }
    }
}
