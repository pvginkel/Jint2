using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal interface ISourceLocation
    {
        SourceLocation Location { get; }
    }
}
