using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundSetAccessor : BoundStatement
    {
        public BoundExpression Expression { get; private set; }
        public BoundExpression Index { get; private set; }
        public BoundExpression GetFunction { get; private set; }
        public BoundExpression SetFunction { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.SetAccessor; }
        }

        public BoundSetAccessor(BoundExpression expression, BoundExpression index, BoundExpression getFunction, BoundExpression setFunction)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (index == null)
                throw new ArgumentNullException("index");

            Debug.Assert(getFunction != null || setFunction != null);

            Expression = expression;
            Index = index;
            GetFunction = getFunction;
            SetFunction = setFunction;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitSetAccessor(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitSetAccessor(this);
        }

        public BoundSetAccessor Update(BoundExpression expression, BoundExpression index, BoundExpression getFunction, BoundExpression setFunction)
        {
            if (
                expression == Expression &&
                index == Index &&
                getFunction == GetFunction &&
                setFunction == SetFunction
            )
                return this;

            return new BoundSetAccessor(expression, index, getFunction, setFunction);
        }
    }
}
