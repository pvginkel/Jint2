using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Ast
{
    internal interface IIdentifier
    {
        string Name { get; }
        int? Index { get; }
        IdentifierType Type { get; }
        Closure Closure { get; }
        WithScope WithScope { get; }
        IIdentifier Fallback { get; }
        bool IsDeclared { get; }
    }
}
