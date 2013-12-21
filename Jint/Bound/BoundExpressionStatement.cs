using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundExpressionStatement : BoundStatement
    {
        public BoundExpression Expression { get; private set; }

        public override BoundNodeType NodeType
        {
            get { return BoundNodeType.ExpressionStatement; }
        }

        public BoundExpressionStatement(BoundExpression expression)
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

        public BoundExpressionStatement Update(BoundExpression expression)
        {
            if (expression == Expression)
                return this;

            return new BoundExpressionStatement(expression);
        }
    }
}
