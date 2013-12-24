using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Expressions;

namespace Jint.Bound
{
    internal class BoundBreak : BoundStatement
    {
        public string Target { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.Break; }
        }

        public BoundBreak(string target, SourceLocation location)
            : base(location)
        {
            Target = target;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitBreak(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitBreak(this);
        }

        public BoundBreak Update(string target, SourceLocation location)
        {
            if (
                target == Target &&
                location == Location
            )
                return this;

            return new BoundBreak(target, location);
        }
    }
}
