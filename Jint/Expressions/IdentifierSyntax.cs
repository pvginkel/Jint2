using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class IdentifierSyntax : ExpressionSyntax, IAssignable
    {
        public IdentifierSyntax(string name)
        {
            Name = name;
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
            visitor.VisitIdentifier(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitIdentifier(this);
        }
    }
}
