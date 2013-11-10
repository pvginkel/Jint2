using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class VariableDeclarationSyntax : SyntaxNode
    {
        public bool Global { get; set; }
        public string Identifier { get; set; }
        public Variable Target { get; set; }
        public ExpressionSyntax Expression { get; set; }

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
