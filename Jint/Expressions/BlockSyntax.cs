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
        public VariableCollection DeclaredVariables { get; private set; }
        public Closure Closure { get; set; }
        public Closure ParentClosure { get; set; }

        public BlockSyntax(IEnumerable<SyntaxNode> statements)
            : this(statements, new VariableCollection())
        {
        }

        public BlockSyntax(IEnumerable<SyntaxNode> statements, VariableCollection declaredVariables)
        {
            if (statements == null)
                throw new ArgumentNullException("statements");
            if (declaredVariables == null)
                throw new ArgumentNullException("declaredVariables");

            Statements = statements.ToReadOnly();
            DeclaredVariables = declaredVariables;
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
