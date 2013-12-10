using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    public class LabelSyntax : SyntaxNode
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Label; }
        }

        public string Label { get; private set; }
        public SyntaxNode Expression { get; private set; }

        public LabelSyntax(string label, SyntaxNode expression)
        {
            if (label == null)
                throw new ArgumentNullException("label");
            if (expression == null)
                throw new ArgumentNullException("expression");

            Label = label;
            Expression = expression;
        }

        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitLabel(this);
        }

        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitLabel(this);
        }
    }
}
