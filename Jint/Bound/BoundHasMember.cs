using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundHasMember : BoundExpression
    {
        public BoundExpression Expression { get; private set; }
        public string Index { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.HasMember; }
        }

        public override BoundValueType ValueType
        {
            get { return BoundValueType.Boolean; }
        }

        public BoundHasMember(BoundExpression expression, string index)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (index == null)
                throw new ArgumentNullException("index");

            Expression = expression;
            Index = index;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitHasMember(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitHasMember(this);
        }

        public BoundHasMember Update(BoundExpression expression, string index)
        {
            if (
                expression == Expression &&
                index == Index
            )
                return this;

            return new BoundHasMember(expression, index);
        }
    }
}
