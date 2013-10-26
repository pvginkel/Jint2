using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class BlockStatement : Statement
    {
        public LinkedList<Statement> Statements { get; private set; }
        public List<string> DeclaredVariables { get; private set; }

        public BlockStatement()
        {
            Statements = new LinkedList<Statement>();
            DeclaredVariables = new List<string>();
        }

        public BlockStatement(BlockStatement other)
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

            foreach (string item in DeclaredVariables)
            {
                if (String.Equals(variableName, item, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            DeclaredVariables.Add(variableName);
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override void Accept(IStatementVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
