using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    [Flags]
    internal enum ArrayShimOptions
    {
        None = 0,
        IncludeMissing = 1
    }
}
