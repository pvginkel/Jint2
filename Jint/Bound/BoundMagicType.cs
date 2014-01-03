using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundMagicType
    {
        public BoundMagicVariableType MagicType { get; private set; }
        public IBoundType Type { get; private set; }

        public BoundMagicType(BoundMagicVariableType magicType, IBoundType type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            MagicType = magicType;
            Type = type;
        }
    }
}
