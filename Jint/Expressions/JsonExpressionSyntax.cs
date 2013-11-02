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
            Values = new Dictionary<string, ExpressionSyntax>();
        }

        public Dictionary<string, ExpressionSyntax> Values { get; set; }

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
            if (Values.ContainsKey(propertyExpression.Name))
            {
                PropertyDeclarationSyntax exp = Values[propertyExpression.Name] as PropertyDeclarationSyntax;
                if (exp == null)
                    throw new JintException("A property cannot be both an accessor and data");
                switch (propertyExpression.Mode)
                {
                    case PropertyExpressionType.Data:
                        if (propertyExpression.Mode == PropertyExpressionType.Data)
                            Values[propertyExpression.Name] = propertyExpression.Expression;
                        else
                            throw new JintException("A property cannot be both an accessor and data");
                        break;
                    case PropertyExpressionType.Get:
                        exp.SetGet(propertyExpression);
                        break;
                    case PropertyExpressionType.Set:
                        exp.SetSet(propertyExpression);
                        break;
                }
            }
            else
            {
                Values.Add(propertyExpression.Name, propertyExpression);
                switch (propertyExpression.Mode)
                {
                    case PropertyExpressionType.Data:
                        Values[propertyExpression.Name] = propertyExpression;
                        break;
                    case PropertyExpressionType.Get:
                        propertyExpression.SetGet(propertyExpression);
                        break;
                    case PropertyExpressionType.Set:
                        propertyExpression.SetSet(propertyExpression);
                        break;
                }
            }
        }
    }
}
