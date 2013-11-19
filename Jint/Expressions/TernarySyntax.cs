using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class TernarySyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Ternary; }
        }

        public ExpressionSyntax Test { get; private set; }
        public ExpressionSyntax Then { get; private set; }
        public ExpressionSyntax Else { get; private set; }

        internal override ValueType ValueType
        {
            get
            {
                if (Then.ValueType == Else.ValueType)
                    return Then.ValueType;

                return ValueType.Unknown;
            }
        }

        public TernarySyntax(ExpressionSyntax test, ExpressionSyntax @then, ExpressionSyntax @else)
        {
            if (test == null)
                throw new ArgumentNullException("test");
            if (then == null)
                throw new ArgumentNullException("then");
            if (@else == null)
                throw new ArgumentNullException("else");

            Test = test;
            Then = then;
            Else = @else;
        }

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
