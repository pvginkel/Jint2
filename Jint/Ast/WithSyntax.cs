using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
{
    internal class WithSyntax : SyntaxNode, ISourceLocation
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.With; }
        }

        public ExpressionSyntax Expression { get; private set; }
        public IIdentifier Identifier { get; private set; }
        public SyntaxNode Body { get; private set; }
        public SourceLocation Location { get; private set; }

        public WithSyntax(ExpressionSyntax expression, IIdentifier identifier, SyntaxNode body, SourceLocation location)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (identifier == null)
                throw new ArgumentNullException("identifier");
            if (body == null)
                throw new ArgumentNullException("body");
            if (location == null)
                throw new ArgumentNullException("location");

            Expression = expression;
            Body = body;
            Identifier = identifier;
            Location = location;
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitWith(this);
        }
    }
}
