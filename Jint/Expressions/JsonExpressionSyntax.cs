using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class JsonExpressionSyntax : ExpressionSyntax
    {
        public IList<JsonProperty> Properties { get; private set; }

        public JsonExpressionSyntax(IEnumerable<JsonProperty> properties)
        {
            if (properties == null)
                throw new ArgumentNullException("properties");

            Properties = properties.ToReadOnly();
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.Json; }
        }

        internal override ValueType ValueType
        {
            get { return ValueType.Unknown; }
        }

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
    }
}
