using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions
{
    internal abstract class ExpressionSyntax : SyntaxNode
    {
        public abstract ValueType ValueType { get; }
    }
}
