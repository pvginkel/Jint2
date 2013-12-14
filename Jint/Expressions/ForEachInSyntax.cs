using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    internal class ForEachInSyntax : SyntaxNode, ISourceLocation
    {
        public string Identifier { get; private set; }
        public Variable Target { get; private set; }
        public ExpressionSyntax Expression { get; private set; }
        public SyntaxNode Body { get; private set; }
        public SourceLocation Location { get; private set; }

        public override SyntaxType Type
        {
            get { return SyntaxType.ForEachIn; }
        }

        public ForEachInSyntax(string identifier, Variable target, ExpressionSyntax expression, SyntaxNode body, SourceLocation location)
        {
            if (identifier == null)
                throw new ArgumentNullException("identifier");
            if (target == null)
                throw new ArgumentNullException("target");
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (body == null)
                throw new ArgumentNullException("body");
            if (location == null)
                throw new ArgumentNullException("location");

            Identifier = identifier;
            Target = target;
            Expression = expression;
            Body = body;
            Location = location;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitForEachIn(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitForEachIn(this);
        }
    }
}
