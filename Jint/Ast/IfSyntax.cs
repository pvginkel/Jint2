using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
{
    internal class IfSyntax : SyntaxNode, ISourceLocation
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.If; }
        }

        public ExpressionSyntax Test { get; private set; }
        public SyntaxNode Then { get; private set; }
        public SyntaxNode Else { get; private set; }
        public SourceLocation Location { get; private set; }

        public IfSyntax(ExpressionSyntax test, SyntaxNode then, SyntaxNode @else, SourceLocation location)
        {
            if (test == null)
                throw new ArgumentNullException("test");
            if (then == null)
                throw new ArgumentNullException("then");
            if (location == null)
                throw new ArgumentNullException("location");

            Test = test;
            Then = then;
            Else = @else;
            Location = location;
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitIf(this);
        }
    }
}
