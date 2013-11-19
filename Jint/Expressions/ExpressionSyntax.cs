using System;

namespace Jint.Expressions
{
    [Serializable]
    public abstract class ExpressionSyntax : SyntaxNode
    {
        internal abstract ValueType ValueType { get; }
    }
}
