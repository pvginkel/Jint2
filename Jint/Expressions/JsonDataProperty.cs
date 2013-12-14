using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal class JsonDataProperty : JsonProperty
    {
        public ExpressionSyntax Expression { get; private set; }

        public override bool IsLiteral
        {
            get { return Expression.IsLiteral; }
        }

        public JsonDataProperty(string name, ExpressionSyntax expression)
            : base(name)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Expression = expression;
        }
    }
}
