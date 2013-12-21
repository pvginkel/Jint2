﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal abstract class BoundVariable : IBoundWritable
    {
        public IBoundType Type { get; private set; }

        public BoundValueType ValueType
        {
            get { return Type.DefinitelyAssigned ? Type.ValueType : BoundValueType.Unknown; }
        }

        protected BoundVariable(IBoundType type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            Type = type;
        }
    }
}
