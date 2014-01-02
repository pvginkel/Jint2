using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
{
    internal class TernarySyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Ternary; }
        }

        public ExpressionSyntax Test { get; private set; }
        public ExpressionSyntax Then { get; private set; }
        public ExpressionSyntax Else { get; private set; }

        public TernarySyntax(ExpressionSyntax test, ExpressionSyntax then, ExpressionSyntax @else)
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
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitTernary(this);
        }

        public override string ToString()
        {
            return Test + " (" + Then + ", " + Else + ")";
        }
    }
}
