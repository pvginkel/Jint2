using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal class BlockSyntax : SyntaxNode
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Block; }
        }

        public IList<SyntaxNode> Statements { get; private set; }

        public BlockSyntax(IEnumerable<SyntaxNode> statements)
        {
            if (statements == null)
                throw new ArgumentNullException("statements");

            Statements = statements.ToReadOnly();
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitBlock(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitBlock(this);
        }
    }
}
