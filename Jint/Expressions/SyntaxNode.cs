using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions
{
    internal abstract class SyntaxNode
    {
        public static readonly IList<SyntaxNode> EmptyList = new SyntaxNode[0];

        public abstract SyntaxType Type { get; }
        public virtual bool IsAssignable { get { return false; } }

        public abstract void Accept(ISyntaxVisitor visitor);

        public abstract T Accept<T>(ISyntaxVisitor<T> visitor);
    }
}
