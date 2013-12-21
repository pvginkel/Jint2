using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundIf : BoundStatement
    {
        public BoundExpression Test { get; private set; }
        public BoundBlock Then { get; private set; }
        public BoundBlock Else { get; private set; }

        public override BoundNodeType NodeType
        {
            get { return BoundNodeType.If; }
        }

        public BoundIf(BoundExpression test, BoundBlock @then, BoundBlock @else)
        {
            if (test == null)
                throw new ArgumentNullException("test");
            if (then == null)
                throw new ArgumentNullException("then");

            Test = test;
            Then = then;
            Else = @else;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitIf(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitIf(this);
        }

        public BoundIf Update(BoundExpression test, BoundBlock @then, BoundBlock @else)
        {
            if (
                test == Test &&
                @then == Then &&
                @else == Else
            )
                return this;

            return new BoundIf(test, @then, @else);
        }
    }
}
