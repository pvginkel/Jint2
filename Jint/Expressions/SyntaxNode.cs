using System;
using System.Collections.Generic;
using Jint.Debugger;

namespace Jint.Expressions
{
    [Serializable]
    public abstract class SyntaxNode
    {
        public static readonly IList<SyntaxNode> EmptyList = new SyntaxNode[0];

        public abstract SyntaxType Type { get; }
        internal virtual bool IsAssignable { get { return false; } }
        internal SourceCodeDescriptor Source { get; set; }

        public abstract void Accept(ISyntaxVisitor visitor);

        public abstract T Accept<T>(ISyntaxVisitor<T> visitor);
    }
}
