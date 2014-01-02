using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Ast
{
    internal class JsonExpressionSyntax : ExpressionSyntax
    {
        public ReadOnlyArray<JsonProperty> Properties { get; private set; }

        public JsonExpressionSyntax(ReadOnlyArray<JsonProperty> properties)
        {
            if (properties == null)
                throw new ArgumentNullException("properties");

            Properties = properties;
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.Json; }
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitJsonExpression(this);
        }
    }
}
