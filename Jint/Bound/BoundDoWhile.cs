using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundDoWhile : BoundStatement
    {
        public BoundExpression Test { get; private set; }
        public BoundBlock Body { get; private set; }

        public override BoundNodeType NodeType
        {
            get { return BoundNodeType.DoWhile; }
        }

        public BoundDoWhile(BoundExpression test, BoundBlock body)
        {
            if (test == null)
                throw new ArgumentNullException("test");
            if (body == null)
                throw new ArgumentNullException("body");

            Test = test;
            Body = body;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitDoWhile(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitDoWhile(this);
        }

        public BoundDoWhile Update(BoundExpression test, BoundBlock body)
        {
            if (
                test == Test &&
                body == Body
            )
                return this;

            return new BoundDoWhile(test, body);
        }
    }
}
