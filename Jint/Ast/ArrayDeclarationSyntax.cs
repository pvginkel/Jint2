using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Ast
{
    internal class ArrayDeclarationSyntax : ExpressionSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.ArrayDeclaration; }
        }

        public ReadOnlyArray<SyntaxNode> Parameters { get; private set; }

        public ArrayDeclarationSyntax(ReadOnlyArray<SyntaxNode> parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            Parameters = parameters;
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitArrayDeclaration(this);
        }
    }
}
