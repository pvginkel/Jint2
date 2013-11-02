using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class ArrayDeclarationSyntax : ExpressionSyntax
    {
        public List<SyntaxNode> Parameters { get; set; }

        public ArrayDeclarationSyntax()
        {
            Parameters = new List<SyntaxNode>();
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitArrayDeclaration(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitArrayDeclaration(this);
        }
    }
}
