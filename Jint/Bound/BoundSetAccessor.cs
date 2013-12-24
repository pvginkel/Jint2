using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Expressions;

namespace Jint.Bound
{
    internal class BoundSetAccessor : BoundStatement
    {
        public BoundExpression Expression { get; private set; }
        public string Name { get; private set; }
        public BoundExpression GetFunction { get; private set; }
        public BoundExpression SetFunction { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.SetAccessor; }
        }

        public BoundSetAccessor(BoundExpression expression, string name, BoundExpression getFunction, BoundExpression setFunction, SourceLocation location)
            : base(location)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (name == null)
                throw new ArgumentNullException("name");

            Debug.Assert(getFunction != null || setFunction != null);

            Expression = expression;
            Name = name;
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

        public BoundSetAccessor Update(BoundExpression expression, string name, BoundExpression getFunction, BoundExpression setFunction, SourceLocation location)
        {
            if (
                expression == Expression &&
                name == Name &&
                getFunction == GetFunction &&
                setFunction == SetFunction &&
                location == Location
            )
                return this;

            return new BoundSetAccessor(expression, name, getFunction, setFunction, location);
        }
    }
}
