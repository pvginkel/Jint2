using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class PropertySyntax : IdentifierSyntax
    {
        public PropertySyntax(string text)
            : base(text)
        {
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
