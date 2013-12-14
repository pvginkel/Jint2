using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    internal class VariableDeclarationSyntax : SyntaxNode, ISourceLocation
    {
        public IList<VariableDeclaration> Declarations { get; private set; }
        public SourceLocation Location { get; private set; }

        public VariableDeclarationSyntax(IEnumerable<VariableDeclaration> declarations, SourceLocation location)
        {
            if (location == null)
                throw new ArgumentNullException("location");

            Declarations = declarations.ToReadOnly();
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
