using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Ast
{
    internal interface ISourceLocation
    {
        SourceLocation Location { get; }
    }
}
