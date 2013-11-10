using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class BlockSyntax : SyntaxNode
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Block; }
        }

        public LinkedList<SyntaxNode> Statements { get; private set; }
        public VariableCollection DeclaredVariables { get; private set; }
        public Closure Closure { get; set; }
        public Closure ParentClosure { get; set; }

        public BlockSyntax()
        {
            Statements = new LinkedList<SyntaxNode>();
            DeclaredVariables = new VariableCollection();
        }

        public BlockSyntax(BlockSyntax other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            Statements = other.Statements;
            DeclaredVariables = other.DeclaredVariables;
        }

        internal Variable DeclareVariable(string variableName)
        {
            if (variableName == null)
                throw new ArgumentNullException("variableName");

            Variable variable;
            if (!DeclaredVariables.TryGetItem(variableName, out variable))
            {
                variable = new Variable(variableName);
                DeclaredVariables.Add(variable);
            }

            return variable;
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
