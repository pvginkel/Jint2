using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    internal class IndexerSyntax : MemberSyntax
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Indexer; }
        }

        public ExpressionSyntax Index { get; private set; }

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
