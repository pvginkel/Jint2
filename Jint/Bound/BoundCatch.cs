using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundCatch : BoundNode
    {
        public IBoundWritable Target { get; private set; }
        public BoundBlock Body { get; private set; }

        public override BoundNodeType NodeType
        {
            get { return BoundNodeType.Catch; }
        }

        public BoundCatch(IBoundWritable target, BoundBlock body)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (body == null)
                throw new ArgumentNullException("body");

            Target = target;
            Body = body;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitCatch(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitCatch(this);
        }

        public BoundCatch Update(IBoundWritable target, BoundBlock body)
        {
            if (
                target == Target &&
                body == Body
            )
                return this;

            return new BoundCatch(target, body);
        }
    }
}
