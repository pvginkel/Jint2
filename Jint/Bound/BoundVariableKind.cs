using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal enum BoundVariableKind
    {
        Magic,
        Argument,
        ClosureField,
        Local,
        Temporary,
        Global
    }
}
