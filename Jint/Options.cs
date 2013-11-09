using System;
using System.Collections.Generic;
using System.Text;

namespace Jint
{
    [Flags]
    public enum Options
    {
        Strict = 1,
        EcmaScript3 = 2,
        EcmaScript5 = 4
    }
}
