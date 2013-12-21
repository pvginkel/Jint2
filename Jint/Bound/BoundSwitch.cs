using Jint.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundSwitch : BoundStatement
    {
        public BoundTemporary Temporary { get; private set; }
        public ReadOnlyArray<BoundSwitchCase> Cases { get; private set; }

        public override BoundNodeType NodeType
        {
            get { return BoundNodeType.Switch; }
        }

        public BoundSwitch(BoundTemporary temporary, ReadOnlyArray<BoundSwitchCase> cases)
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

        public BoundSwitch Update(BoundTemporary temporary, ReadOnlyArray<BoundSwitchCase> cases)
        {
            if (
                temporary == Temporary &&
                cases == Cases
            )
                return this;

            return new BoundSwitch(temporary, cases);
        }
    }
}
