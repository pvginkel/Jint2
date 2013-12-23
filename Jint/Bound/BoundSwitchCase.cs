using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundSwitchCase : BoundNode
    {
        public BoundExpression Expression { get; private set; }
        public BoundBlock Body { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.SwitchCase; }
        }

        public BoundSwitchCase(BoundExpression expression, BoundBlock body)
        {
            Debug.Assert(expression != null || body != null);

            Expression = expression;
            Body = body;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitSwitchCase(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitSwitchCase(this);
        }

        public BoundSwitchCase Update(BoundExpression expression, BoundBlock body)
        {
            if (
                expression == Expression &&
                body == Body
            )
                return this;

            return new BoundSwitchCase(expression, body);
        }
    }
}
