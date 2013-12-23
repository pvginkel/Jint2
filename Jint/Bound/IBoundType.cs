﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal interface IBoundType
    {
        BoundValueType Type { get; }
        bool DefinitelyAssigned { get; }
        BoundTypeKind Kind { get; }

        void MarkUnused();
    }
}
