using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    public enum VariableType
    {
        Unknown,
        Parameter,
        Local,
        This,
        Arguments,
        Global,
        WithScope
    }
}
