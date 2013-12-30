using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    [Flags]
    public enum JsRegexOptions
    {
        None = 0,
        Global = 1,
        IgnoreCase = 2,
        Multiline = 4
    }
}
