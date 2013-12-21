using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundBreak : BoundStatement
    {
        public string Target { get; private set; }

        public override BoundNodeType NodeType
        {
            get { return BoundNodeType.Break; }
        }

        public BoundBreak(string target)
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

        public BoundBreak Update(string target)
        {
            if (target == Target)
                return this;

            return new BoundBreak(target);
        }
    }
}
