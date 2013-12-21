using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundSetVariable : BoundStatement
    {
        public IBoundWritable Variable { get; private set; }
        public BoundExpression Value { get; private set; }

        public override BoundNodeType NodeType
        {
            get { return BoundNodeType.SetVariable; }
        }

        public BoundSetVariable(IBoundWritable variable, BoundExpression value)
        {
            if (variable == null)
                throw new ArgumentNullException("variable");
            if (value == null)
                throw new ArgumentNullException("value");

            Variable = variable;
            Value = value;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitSetVariable(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitSetVariable(this);
        }

        public BoundSetVariable Update(IBoundWritable variable, BoundExpression value)
        {
            if (
                variable == Variable &&
                value == Value
            )
                return this;

            return new BoundSetVariable(variable, value);
        }
    }
}
