using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Ast
{
    internal enum IdentifierType
    {
        Unknown,
        Parameter,
        Local,
        This,
        Null,
        Undefined,
        Arguments,
        Global,
        Scoped
    }
}
