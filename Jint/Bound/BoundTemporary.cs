using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundTemporary : IBoundWritable, IHasBoundType
    {
        public int Index { get; private set; }
        public IBoundType Type { get; set; }

        public BoundTemporary(int index, IBoundType type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            Index = index;
            Type = type;
        }

        public override string ToString()
        {
            return "Temporary(" + Index + ")";
        }
    }
}
