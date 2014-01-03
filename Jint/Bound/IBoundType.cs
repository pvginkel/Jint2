using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal interface IBoundType
    {
        string Name { get; }
        BoundValueType Type { get; }
        SpeculatedType SpeculatedType { get; }
        bool DefinitelyAssigned { get; }
        BoundTypeKind Kind { get; }

        void MarkUnused();
    }
}
