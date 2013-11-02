using System;
using Jint.Debugger;

namespace Jint.Expressions
{
    [Serializable]
    public abstract class SyntaxNode
    {
        public string Label { get; set; }

        public abstract void Accept(ISyntaxVisitor visitor);
        public abstract T Accept<T>(ISyntaxVisitor<T> visitor);

        public SourceCodeDescriptor Source { get; set; }

        protected SyntaxNode()
        {
            Label = String.Empty;
        }
    }
}
