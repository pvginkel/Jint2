using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundWhile : BoundStatement
    {
        public BoundExpression Test { get; private set; }
        public BoundBlock Body { get; private set; }

        public override BoundNodeType NodeType
        {
            get { return BoundNodeType.While; }
        }

        public BoundWhile(BoundExpression test, BoundBlock body)
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
            visitor.VisitWhile(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitWhile(this);
        }

        public BoundWhile Update(BoundExpression test, BoundBlock body)
        {
            if (
                test == Test &&
                body == Body
            )
                return this;

            return new BoundWhile(test, body);
        }
    }
}
