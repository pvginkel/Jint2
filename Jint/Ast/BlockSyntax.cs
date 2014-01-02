using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Ast
{
    internal class BlockSyntax : SyntaxNode
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Block; }
        }

        public ReadOnlyArray<SyntaxNode> Statements { get; private set; }

        public BlockSyntax(ReadOnlyArray<SyntaxNode> statements)
        {
            if (statements == null)
                throw new ArgumentNullException("statements");

            Statements = statements;
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitBlock(this);
        }
    }
}
