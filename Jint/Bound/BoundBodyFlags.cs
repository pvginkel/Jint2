using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    [Flags]
    internal enum BoundBodyFlags
    {
        None = 0,
        ArgumentsReferenced = 1,
        Strict = 2
    }
}
