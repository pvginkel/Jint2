using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class ClrIdentifierSyntax : ExpressionSyntax
    {
        public ClrIdentifierSyntax(string text)
        {
            Text = text;
        }

        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitClrIdentifier(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitClrIdentifier(this);
        }
    }
}
