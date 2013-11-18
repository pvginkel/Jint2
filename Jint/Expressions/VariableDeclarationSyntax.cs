using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class VariableDeclarationSyntax : SyntaxNode
    {
        public string Identifier { get; private set; }
        public ExpressionSyntax Expression { get; private set; }
        public bool Global { get; private set; }
        internal Variable Target { get; set; }

        public VariableDeclarationSyntax(string identifier, ExpressionSyntax expression, bool global)
        {
            if (identifier == null)
                throw new ArgumentNullException("identifier");

            Identifier = identifier;
            Expression = expression;
            Global = global;
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
