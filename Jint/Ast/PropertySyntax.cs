using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
{
    internal class PropertySyntax : MemberSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Property; }
        }

        public string Name { get; private set; }

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
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitProperty(this);
        }
    }
}
