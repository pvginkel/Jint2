using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
{
    internal class ForEachInSyntax : SyntaxNode, ISourceLocation
    {
        public IIdentifier Identifier { get; private set; }
        public ExpressionSyntax Expression { get; private set; }
        public SyntaxNode Body { get; private set; }
        public SourceLocation Location { get; private set; }

        public override SyntaxType Type
        {
            get { return SyntaxType.ForEachIn; }
        }

        public ForEachInSyntax(IIdentifier identifier, ExpressionSyntax expression, SyntaxNode body, SourceLocation location)
        {
            if (identifier == null)
                throw new ArgumentNullException("identifier");
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (body == null)
                throw new ArgumentNullException("body");
            if (location == null)
                throw new ArgumentNullException("location");

            Identifier = identifier;
            Expression = expression;
            Body = body;
            Location = location;
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitForEachIn(this);
        }
    }
}
