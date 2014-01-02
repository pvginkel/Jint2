using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Ast;

namespace Jint.Bound
{
    internal class BoundContinue : BoundStatement
    {
        public string Target { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.Continue; }
        }

        public BoundContinue(string target, SourceLocation location)
            : base(location)
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

        public BoundContinue Update(string target, SourceLocation location)
        {
            if (
                target == Target &&
                location == Location
            )
                return this;

            return new BoundContinue(target, location);
        }
    }
}
