using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal interface IHasBoundType
    {
        IBoundType Type { get; }
    }
}
