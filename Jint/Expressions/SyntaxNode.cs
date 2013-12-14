using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions
{
    public abstract class SyntaxNode
    {
        public static readonly IList<SyntaxNode> EmptyList = new SyntaxNode[0];

        public abstract SyntaxType Type { get; }
        internal virtual bool IsAssignable { get { return false; } }
        internal virtual bool IsLiteral { get { return false; } }

        public abstract void Accept(ISyntaxVisitor visitor);

        public abstract T Accept<T>(ISyntaxVisitor<T> visitor);
    }
}
