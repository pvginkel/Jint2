using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundEmpty : BoundStatement
    {
        public override BoundKind Kind
        {
            get { return BoundKind.Empty; }
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitEmpty(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitEmpty(this);
        }

        public BoundEmpty Update()
        {
            return this;
        }
    }
}
