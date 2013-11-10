using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class IndexerSyntax : MemberSyntax
    {
        public IndexerSyntax(ExpressionSyntax expression)
        {
            Expression = expression;
        }

        public IndexerSyntax(ExpressionSyntax expression, ExpressionSyntax index)
            : this(expression)
        {
            Index = index;
        }

        public override SyntaxType Type
        {
            get { return SyntaxType.Indexer; }
        }

        public ExpressionSyntax Index { get; set; }

        public override string ToString()
        {
            return "[" + Index + "]" + base.ToString();
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
