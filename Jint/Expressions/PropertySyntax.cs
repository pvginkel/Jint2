using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class PropertySyntax : MemberSyntax
    {
        public PropertySyntax(ExpressionSyntax expression, string name)
        {
            Expression = expression;
            Name = name;
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.Property; }
        }

        public string Name { get; set; }
        public Variable Target { get; set; }

        public override string ToString()
        {
            return Name;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitProperty(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitProperty(this);
        }
    }
}
