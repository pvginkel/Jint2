using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class TernarySyntax : ExpressionSyntax
    {
        public TernarySyntax(ExpressionSyntax test, ExpressionSyntax @then, ExpressionSyntax @else)
        {
            Test = test;
            Then = then;
            Else = @else;
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.Ternary; }
        }

        public ExpressionSyntax Test { get; set; }

        public ExpressionSyntax Then { get; set; }

        public ExpressionSyntax Else { get; set; }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitTernary(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitTernary(this);
        }

        public override string ToString()
        {
            return Test + " (" + Then + ", " + Else + ")";
        }
    }
}
