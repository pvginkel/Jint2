using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class IndexerSyntax : ExpressionSyntax, IAssignable
    {
        public IndexerSyntax()
        {
        }

        public IndexerSyntax(ExpressionSyntax index)
        {
            Expression = index;
        }

        public ExpressionSyntax Expression { get; set; }

        public override string ToString()
        {
            return "[" + Expression + "]" + base.ToString();
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitIndexer(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitIndexer(this);
        }
    }
}
