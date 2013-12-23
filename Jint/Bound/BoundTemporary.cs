using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundTemporary : BoundVariable
    {
        public int Index { get; private set; }

        public override BoundVariableKind Kind
        {
            get { return BoundVariableKind.Temporary; }
        }

        public BoundTemporary(int index, IBoundType type)
            : base(type)
        {
            Index = index;
        }

        public override string ToString()
        {
            return "Temporary(" + Index + ")";
        }
    }
}
