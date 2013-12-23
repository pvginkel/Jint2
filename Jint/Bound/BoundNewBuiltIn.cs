using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundNewBuiltIn : BoundExpression
    {
        public BoundNewBuiltInType NewBuiltInType { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.NewBuiltIn; }
        }

        public override BoundValueType ValueType
        {
            get { return BoundValueType.Object; }
        }

        public BoundNewBuiltIn(BoundNewBuiltInType newBuiltInType)
        {
            NewBuiltInType = newBuiltInType;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitNewBuiltIn(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitNewBuiltIn(this);
        }

        public BoundNewBuiltIn Update(BoundNewBuiltInType newBuiltInType)
        {
            if (newBuiltInType == NewBuiltInType)
                return this;

            return new BoundNewBuiltIn(newBuiltInType);
        }
    }
}
