using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
{
    internal class VariableDeclarationSyntax : SyntaxNode, ISourceLocation
    {
        public ReadOnlyArray<VariableDeclaration> Declarations { get; private set; }
        public SourceLocation Location { get; private set; }

        public VariableDeclarationSyntax(ReadOnlyArray<VariableDeclaration> declarations, SourceLocation location)
        {
            if (location == null)
                throw new ArgumentNullException("location");

            Declarations = declarations;
            Location = location;
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.VariableDeclaration; }
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitVariableDeclaration(this);
        }
    }
}
