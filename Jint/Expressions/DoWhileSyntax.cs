using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class DoWhileSyntax : SyntaxNode
    {
        public ExpressionSyntax Test { get; set; }
        public SyntaxNode Body { get; set; }

        public DoWhileSyntax(ExpressionSyntax condition, SyntaxNode statement)
        {
            Test = condition;
            Body = statement;
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.DoWhile; }
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitDoWhile(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitDoWhile(this);
        }
    }
}
