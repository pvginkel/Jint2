using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Expressions
{
    public interface IFunctionDeclaration
    {
        string Name { get; set; }
        List<string> Parameters { get; set; }
        BlockSyntax Body { get; set; }
    }
}
