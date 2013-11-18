using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class DoWhileSyntax : SyntaxNode
    {
        public ExpressionSyntax Test { get; private set; }
        public SyntaxNode Body { get; private set; }

        public override SyntaxType Type
        {
            get { return SyntaxType.DoWhile; }
        }

        public DoWhileSyntax(ExpressionSyntax condition, SyntaxNode statement)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");
            if (statement == null)
                throw new ArgumentNullException("statement");

            Test = condition;
            Body = statement;
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
