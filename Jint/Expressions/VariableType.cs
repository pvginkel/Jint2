using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal enum VariableType
    {
        Unknown,
        Parameter,
        Local,
        This,
        Null,
        Undefined,
        Arguments,
        Global,
        WithScope
    }
}
