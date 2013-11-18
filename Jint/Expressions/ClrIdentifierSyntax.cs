using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class ClrIdentifierSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.ClrIdentifier; }
        }

        public string Text { get; private set; }

        public ClrIdentifierSyntax(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            Text = text;
        }

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
