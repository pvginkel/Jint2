using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    public interface ISourceLocation
    {
        SourceLocation Location { get; }
    }
}
