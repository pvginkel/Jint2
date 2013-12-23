using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundEmitExpression : BoundExpression
    {
        private readonly BoundValueType _valueType;
        private readonly Action _emit;

        public override BoundValueType ValueType { get { return _valueType; } }

        public override BoundKind Kind
        {
            get { return BoundKind.Emit; }
        }

        public BoundEmitExpression(BoundValueType valueType, Action emit)
        {
            if (emit == null)
                throw new ArgumentNullException("emit");

            _valueType = valueType;
            _emit = emit;
        }

        public override void Accept(BoundTreeVisitor visitor)
        {
            // This is a helper expression used to allow Emit's to be passed
            // as expressions.
            throw new InvalidOperationException();
        }

        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            // This is a helper expression used to allow Emit's to be passed
            // as expressions.
            throw new InvalidOperationException();
        }

        public void Emit()
        {
            _emit();
        }
    }
}
