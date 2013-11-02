using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class WhileSyntax : SyntaxNode
    {
        public ExpressionSyntax Test { get; set; }
        public SyntaxNode Body { get; set; }

        public WhileSyntax(ExpressionSyntax condition, SyntaxNode statement)
        {
            Test = condition;
            Body = statement;
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitWhile(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitWhile(this);
        }
    }
}
