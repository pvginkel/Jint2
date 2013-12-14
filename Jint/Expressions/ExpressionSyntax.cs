using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions
{
    public abstract class ExpressionSyntax : SyntaxNode
    {
        internal abstract ValueType ValueType { get; }
    }
}
