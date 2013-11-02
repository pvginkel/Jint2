using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class NewSyntax : ExpressionSyntax, IGenericExpression
    {
        // public List<Identifier> Identifiers { get; set; }
        public ExpressionSyntax Expression { get; set; }
        public List<ExpressionSyntax> Arguments { get; set; }
        public List<ExpressionSyntax> Generics { get; set; }

        public NewSyntax(ExpressionSyntax expression)
        {
            Expression = expression;

            // Identifiers = new List<Identifier>();
            Arguments = new List<ExpressionSyntax>();
            Generics = new List<ExpressionSyntax>();
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitNew(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitNew(this);
        }
    }
}
