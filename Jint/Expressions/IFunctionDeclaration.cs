using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions
{
    public interface IFunctionDeclaration
    {
        string Name { get; }
        IList<string> Parameters { get; }
        BlockSyntax Body { get; }
    }
}
