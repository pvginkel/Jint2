using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundConstant : BoundExpression
    {
        public static readonly BoundConstant True = new BoundConstant(true);
        public static readonly BoundConstant False = new BoundConstant(false);
        public static readonly BoundConstant EmptyString = new BoundConstant("");

        public override BoundNodeType NodeType
        {
            get { return BoundNodeType.Constant; }
        }

        public static BoundConstant Create(bool value)
        {
            return value ? True : False;
        }

        public static BoundConstant Create(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (value == "")
                return EmptyString;

            return new BoundConstant(value);
        }

        public static BoundConstant Create(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (value is bool)
                return (bool)value ? True : False;

            if (value is string && (string)value == "")
                return EmptyString;

            return new BoundConstant(value);
        }

        public object Value { get; private set; }

        private BoundConstant(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            Value = value;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitConstant(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitConstant(this);
        }

        public BoundConstant Update(object value)
        {
            if (value == Value)
                return this;

            return Create(value);
        }
    }
}
