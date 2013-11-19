using System;
using System.Diagnostics;

namespace Jint.Expressions
{
    [Serializable]
    public class IndexerSyntax : MemberSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Indexer; }
        }

        public ExpressionSyntax Index { get; private set; }

        internal override ValueType ValueType
        {
            get { return ValueType.Unknown; }
        }

        public IndexerSyntax(ExpressionSyntax expression, ExpressionSyntax index)
            : base(expression)
        {
            if (index == null)
                throw new ArgumentNullException("index");

            Index = index;
        }

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
