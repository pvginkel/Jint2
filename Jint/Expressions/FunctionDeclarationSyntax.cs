using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class FunctionDeclarationSyntax : SyntaxNode, IFunctionDeclaration
    {
        public string Name { get; set; }
        public Variable Target { get; set; }
        public List<string> Parameters { get; set; }
        public BlockSyntax Body { get; set; }

        public FunctionDeclarationSyntax()
        {
            Parameters = new List<string>();
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitFunctionDeclaration(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitFunctionDeclaration(this);
        }
    }
}
