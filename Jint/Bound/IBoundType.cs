using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal interface IBoundType
    {
        BoundValueType ValueType { get; }
        bool DefinitelyAssigned { get; }
        BoundTypeType Type { get; }

        void MarkUnused();
    }
}
