using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    partial class BoundTypeManager
    {
        public class TypeMarker
        {
            public void MarkWrite(IBoundType type, BoundValueType valueType)
            {
                var internalType = (BoundType)type;

                if (type.ValueType == BoundValueType.Unset)
                    internalType.ValueType = valueType;
                else if (type.ValueType != valueType)
                    internalType.ValueType = BoundValueType.Unknown;
            }
        }
    }
}
