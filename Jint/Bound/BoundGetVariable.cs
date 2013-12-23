using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundGetVariable : BoundExpression
    {
        public IBoundReadable Variable { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.GetVariable; }
        }

        public override BoundValueType ValueType
        {
            get { return Variable.ValueType; }
        }

        public BoundGetVariable(IBoundReadable variable)
        {
            if (variable == null)
                throw new ArgumentNullException("variable");

            Variable = variable;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitGetVariable(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitGetVariable(this);
        }

        public BoundGetVariable Update(IBoundReadable variable)
        {
            if (variable == Variable)
                return this;

            return new BoundGetVariable(variable);
        }
    }
}
