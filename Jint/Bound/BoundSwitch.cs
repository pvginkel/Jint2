using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Ast;

namespace Jint.Bound
{
    internal class BoundSwitch : BoundStatement
    {
        public BoundTemporary Temporary { get; private set; }
        public ReadOnlyArray<BoundSwitchCase> Cases { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.Switch; }
        }

        public BoundSwitch(BoundTemporary temporary, ReadOnlyArray<BoundSwitchCase> cases, SourceLocation location)
            : base(location)
        {
            if (temporary == null)
                throw new ArgumentNullException("temporary");
            if (cases == null)
                throw new ArgumentNullException("cases");

            Temporary = temporary;
            Cases = cases;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitSwitch(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitSwitch(this);
        }

        public BoundSwitch Update(BoundTemporary temporary, ReadOnlyArray<BoundSwitchCase> cases, SourceLocation location)
        {
            if (
                temporary == Temporary &&
                cases == Cases &&
                location == Location
            )
                return this;

            return new BoundSwitch(temporary, cases, location);
        }
    }
}
