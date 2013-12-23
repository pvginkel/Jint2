using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundNew : BoundExpression
    {
        public BoundExpression Expression { get; private set; }
        public ReadOnlyArray<BoundCallArgument> Arguments { get; private set; }
        public ReadOnlyArray<BoundExpression> Generics { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.New; }
        }

        public override BoundValueType ValueType
        {
            get { return BoundValueType.Object; }
        }

        public BoundNew(BoundExpression expression, ReadOnlyArray<BoundCallArgument> arguments, ReadOnlyArray<BoundExpression> generics)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (arguments == null)
                throw new ArgumentNullException("arguments");
            if (generics == null)
                throw new ArgumentNullException("generics");

            Expression = expression;
            Arguments = arguments;
            Generics = generics;
        }

        [DebuggerStepThrough]
        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitNew(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitNew(this);
        }

        public BoundNew Update(BoundExpression expression, ReadOnlyArray<BoundCallArgument> arguments, ReadOnlyArray<BoundExpression> generics)
        {
            if (
                expression == Expression &&
                arguments == Arguments &&
                generics == Generics
            )
                return this;

            return new BoundNew(expression, arguments, generics);
        }
    }
}
