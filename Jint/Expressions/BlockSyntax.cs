using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class BlockSyntax : SyntaxNode
    {
        public LinkedList<SyntaxNode> Statements { get; private set; }
        public List<string> DeclaredVariables { get; private set; }

        public BlockSyntax()
        {
            Statements = new LinkedList<SyntaxNode>();
            DeclaredVariables = new List<string>();
        }

        public BlockSyntax(BlockSyntax other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            Statements = other.Statements;
            DeclaredVariables = other.DeclaredVariables;
        }

        internal void DeclareVariable(string variableName)
        {
            if (variableName == null)
                throw new ArgumentNullException("variableName");

            if (!DeclaredVariables.Contains(variableName))
                DeclaredVariables.Add(variableName);
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
