using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class PropertyDeclarationSyntax : ExpressionSyntax
    {
        public string Name { get; set; }

        public ExpressionSyntax Expression { get; set; }

        public PropertyExpressionType Mode { get; set; }

        public ExpressionSyntax GetExpression { get; set; }

        public ExpressionSyntax SetExpression { get; set; }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitPropertyDeclaration(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitPropertyDeclaration(this);
        }

        internal void SetSet(PropertyDeclarationSyntax propertyExpression)
        {
            SetExpression = propertyExpression.Expression;
        }

        internal void SetGet(PropertyDeclarationSyntax propertyExpression)
        {
            GetExpression = propertyExpression.Expression;
        }
    }
}
