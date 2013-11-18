using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions
{
    public interface IForStatement
    {
        SyntaxNode Initialization { get; }
        SyntaxNode Body { get; }
    }
}
