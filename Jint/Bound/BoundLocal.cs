using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundLocal : IBoundWritable, IHasBoundType
    {
        public string Name { get; private set; }
        public IBoundType Type { get; private set; }

        public BoundLocal(string name, IBoundType type)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (type == null)
                throw new ArgumentNullException("type");

            Name = name;
            Type = type;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
