using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    internal class PropertySyntax : MemberSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Property; }
        }

        public string Name { get; private set; }

        public override ValueType ValueType
        {
            get { return ValueType.Unknown; }
        }

        public PropertySyntax(ExpressionSyntax expression, string name)
            : base(expression)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Name = name;
        }

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
