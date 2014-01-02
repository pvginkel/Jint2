using System;
using System.Collections.Generic;
using System.Text;
using Jint.Ast;

namespace Jint.Parser
{
    internal class JsonPropertyBuilder
    {
        private readonly Dictionary<string, Assignment> _assignments = new Dictionary<string, Assignment>();

        public void AddProperty(PropertyDeclaration propertyExpression)
        {
            string name = propertyExpression.Name;
            var mode = propertyExpression.Mode;

            if (name == null)
            {
                name = mode.ToString().ToLower();
                mode = PropertyExpressionType.Data;
            }

            Assignment declaration;
            if (_assignments.TryGetValue(name, out declaration))
            {
                if (
                    (declaration.Mode == PropertyExpressionType.Data) !=
                    (mode == PropertyExpressionType.Data)
                )
                    throw new JintException("A property cannot be both an accessor and data");
            }
            else
            {
                declaration = new Assignment
                {
                    Mode = mode,
                    Expression = propertyExpression.Expression
                };

                _assignments.Add(name, declaration);
            }

            switch (mode)
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

        public ReadOnlyArray<JsonProperty> GetProperties()
        {
            var builder = new ReadOnlyArray<JsonProperty>.Builder();

            foreach (var assignment in _assignments)
            {
                if (assignment.Value.Mode == PropertyExpressionType.Data)
                {
                    builder.Add(new JsonDataProperty(
                        assignment.Key,
                        assignment.Value.Expression
                    ));
                }
                else
                {
                    builder.Add(new JsonAccessorProperty(
                        assignment.Key,
                        assignment.Value.GetExpression,
                        assignment.Value.SetExpression
                    ));
                }
            }

            return builder.ToReadOnly();
        }

        private class Assignment
        {
            public PropertyExpressionType Mode { get; set; }
            public ExpressionSyntax Expression { get; set; }
            public ExpressionSyntax GetExpression { get; set; }
            public ExpressionSyntax SetExpression { get; set; }
        }
    }
}
