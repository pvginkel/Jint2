using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundCreateFunction : BoundExpression
    {
        public BoundFunction Function { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.CreateFunction; }
        }

        public override BoundValueType ValueType
        {
            get { return BoundValueType.Object; }
        }

        public BoundCreateFunction(BoundFunction function)
        {
            if (function == null)
                throw new ArgumentNullException("function");

            Function = function;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitCreateFunction(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitCreateFunction(this);
        }

        public BoundCreateFunction Update(BoundFunction function)
        {
            if (function == Function)
                return this;

            return new BoundCreateFunction(function);
        }
    }
}
