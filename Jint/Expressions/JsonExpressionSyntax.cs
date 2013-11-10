using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class JsonExpressionSyntax : ExpressionSyntax
    {
        public JsonExpressionSyntax()
        {
            Values = new Dictionary<string, PropertyDeclarationSyntax>();
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.Json; }
        }

        public Dictionary<string, PropertyDeclarationSyntax> Values { get; private set; }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitJsonExpression(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitJsonExpression(this);
        }

        internal void Push(PropertyDeclarationSyntax propertyExpression)
        {
            if (propertyExpression.Name == null)
            {
                propertyExpression.Name = propertyExpression.Mode.ToString().ToLower();
                propertyExpression.Mode = PropertyExpressionType.Data;
            }

            PropertyDeclarationSyntax declaration;
            if (Values.TryGetValue(propertyExpression.Name, out declaration))
            {
                if ((declaration.Mode == PropertyExpressionType.Data) != (propertyExpression.Mode == PropertyExpressionType.Data))
                    throw new JintException("A property cannot be both an accessor and data");
            }
            else
            {
                declaration = propertyExpression;
                Values.Add(propertyExpression.Name, propertyExpression);
            }

            switch (propertyExpression.Mode)
            {
                case PropertyExpressionType.Get:
                    declaration.GetExpression = propertyExpression.Expression;
                    declaration.Expression = null;
                    break;

                case PropertyExpressionType.Set:
                    declaration.SetExpression = propertyExpression.Expression;
                    declaration.Expression = null;
                    break;
            }
        }
    }
}
