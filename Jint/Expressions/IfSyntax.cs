using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    public class IfSyntax : SyntaxNode
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.If; }
        }

        public ExpressionSyntax Test { get; private set; }
        public SyntaxNode Then { get; private set; }
        public SyntaxNode Else { get; private set; }

        public IfSyntax(ExpressionSyntax test, SyntaxNode then, SyntaxNode @else)
        {
            if (test == null)
                throw new ArgumentNullException("test");
            if (then == null)
                throw new ArgumentNullException("then");

            Test = test;
            Then = then;
            Else = @else;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitIf(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitIf(this);
        }
    }
}
