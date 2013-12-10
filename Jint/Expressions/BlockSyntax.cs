using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Compiler;

namespace Jint.Expressions
{
    public class BlockSyntax : SyntaxNode
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Block; }
        }

        public IList<SyntaxNode> Statements { get; private set; }
        internal VariableCollection DeclaredVariables { get; private set; }
        internal Closure Closure { get; set; }
        internal Closure ParentClosure { get; set; }

        public BlockSyntax(IEnumerable<SyntaxNode> statements)
            : this(statements, new VariableCollection())
        {
        }

        internal BlockSyntax(IEnumerable<SyntaxNode> statements, VariableCollection declaredVariables)
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
