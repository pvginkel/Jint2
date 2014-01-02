using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Ast
{
    internal class EmptySyntax : SyntaxNode
    {
        public override SyntaxType Type
        {
            get { return SyntaxType.Empty; }
        }

        [DebuggerStepThrough]
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitEmpty(this);
        }
    }
}
