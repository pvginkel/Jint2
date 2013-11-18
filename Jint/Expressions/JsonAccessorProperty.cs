using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    public class JsonAccessorProperty : JsonProperty
    {
        public ExpressionSyntax GetExpression { get; private set; }
        public ExpressionSyntax SetExpression { get; private set; }

        public JsonAccessorProperty(string name, ExpressionSyntax getExpression, ExpressionSyntax setExpression)
            : base(name)
        {
            GetExpression = getExpression;
            SetExpression = setExpression;
        }
    }
}
