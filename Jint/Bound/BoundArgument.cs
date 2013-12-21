using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundArgument : IBoundWritable
    {
        public string Name { get; private set; }
        public int Index { get; private set; }

        public BoundValueType ValueType
        {
            get { return BoundValueType.Unknown; }
        }

        public BoundArgument(string name, int index)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Name = name;
            Index = index;
        }

        public override string ToString()
        {
            return "Argument(" + Name + ")";
        }
    }
}
