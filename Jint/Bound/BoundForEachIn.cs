using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Ast;

namespace Jint.Bound
{
    internal class BoundForEachIn : BoundStatement
    {
        public IBoundWritable Target { get; private set; }
        public BoundExpression Expression { get; private set; }
        public BoundBlock Body { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.ForEachIn; }
        }

        public BoundForEachIn(IBoundWritable target, BoundExpression expression, BoundBlock body, SourceLocation location)
            : base(location)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (body == null)
                throw new ArgumentNullException("body");

            Target = target;
            Expression = expression;
            Body = body;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitForEachIn(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitForEachIn(this);
        }

        public BoundForEachIn Update(IBoundWritable target, BoundExpression expression, BoundBlock body, SourceLocation location)
        {
            if (
                target == Target &&
                expression == Expression &&
                body == Body &&
                location == Location
            )
                return this;

            return new BoundForEachIn(target, expression, body, location);
        }
    }
}
