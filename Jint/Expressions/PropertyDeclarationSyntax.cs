using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class PropertyDeclarationSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.PropertyDeclaration; }
        }

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
    }
}
