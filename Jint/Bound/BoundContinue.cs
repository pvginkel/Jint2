using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundContinue : BoundStatement
    {
        public string Target { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.Continue; }
        }

        public BoundContinue(string target)
        {
            Target = target;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitContinue(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitContinue(this);
        }

        public BoundContinue Update(string target)
        {
            if (target == Target)
                return this;

            return new BoundContinue(target);
        }
    }
}
