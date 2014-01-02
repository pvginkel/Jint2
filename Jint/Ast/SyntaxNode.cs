using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Ast
{
    internal abstract class SyntaxNode
    {
        public abstract SyntaxType Type { get; }
        public virtual bool IsAssignable { get { return false; } }

        public abstract T Accept<T>(ISyntaxTreeVisitor<T> visitor);
    }
}
