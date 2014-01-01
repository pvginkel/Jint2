using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Expressions
{
    internal class ProgramSyntax : SyntaxNode
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Program; }
        }

        public BodySyntax Body { get; private set; }

        public ProgramSyntax(BodySyntax body)
        {
            if (body == null)
                throw new ArgumentNullException("body");

            Body = body;
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
