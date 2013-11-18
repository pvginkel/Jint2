using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    [Serializable]
    public class ProgramSyntax : BlockSyntax
    {
        public ProgramSyntax(IEnumerable<SyntaxNode> statements)
            : this(statements, null)
        {
        }

        internal ProgramSyntax(IEnumerable<SyntaxNode> statements, VariableCollection declaredVariables)
            : base(statements, declaredVariables)
        {
        }

        [DebuggerStepThrough]
        public override void Accept(ISyntaxVisitor visitor)
        {
            visitor.VisitProgram(this);
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxVisitor<T> visitor)
        {
            return visitor.VisitProgram(this);
        }
    }
}
