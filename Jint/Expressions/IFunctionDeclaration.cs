using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions
{
    public interface IFunctionDeclaration : ISourceLocation
    {
        string Name { get; }
        IList<string> Parameters { get; }
        BlockSyntax Body { get; }
    }
}
