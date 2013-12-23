using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundFinally : BoundNode
    {
        public BoundBlock Body { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.Finally; }
        }

        public BoundFinally(BoundBlock body)
        {
            if (body == null)
                throw new ArgumentNullException("body");

            Body = body;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitFinally(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitFinally(this);
        }

        public BoundFinally Update(BoundBlock body)
        {
            if (body == Body)
                return this;

            return new BoundFinally(body);
        }
    }
}
