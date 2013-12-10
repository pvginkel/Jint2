using System;

namespace Jint.Expressions
{
    public abstract class ExpressionSyntax : SyntaxNode
    {
        internal abstract ValueType ValueType { get; }
    }
}
