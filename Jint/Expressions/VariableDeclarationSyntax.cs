using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Jint.Compiler;

namespace Jint.Expressions
{
    public class VariableDeclarationSyntax : SyntaxNode, ISourceLocation
    {
        public string Identifier { get; private set; }
        public ExpressionSyntax Expression { get; private set; }
        public bool Global { get; private set; }
        internal Variable Target { get; set; }
        public SourceLocation Location { get; private set; }

        public VariableDeclarationSyntax(string identifier, ExpressionSyntax expression, bool global, SourceLocation location)
        {
            if (identifier == null)
                throw new ArgumentNullException("identifier");
            if (location == null)
                throw new ArgumentNullException("location");

            Identifier = identifier;
            Expression = expression;
            Global = global;
            Location = location;
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.VariableDeclaration; }
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitVariableDeclaration(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitVariableDeclaration(this);
        }
    }
}
