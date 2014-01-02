using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Ast;

namespace Jint.Bound
{
    internal class BoundExpressionStatement : BoundStatement
    {
        public BoundExpression Expression { get; private set; }

        public override BoundKind Kind
        {
            get { return BoundKind.ExpressionStatement; }
        }

        public BoundExpressionStatement(BoundExpression expression, SourceLocation location)
            : base(location)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Expression = expression;
        }

        public override void Accept(BoundTreeVisitor visitor)
        {
            visitor.VisitExpressionStatement(this);
        }

        public override T Accept<T>(BoundTreeVisitor<T> visitor)
        {
            return visitor.VisitExpressionStatement(this);
        }

        public BoundExpressionStatement Update(BoundExpression expression, SourceLocation location)
        {
            if (
                expression == Expression &&
                location == Location
            )
                return this;

            return new BoundExpressionStatement(expression, location);
        }
    }
}
