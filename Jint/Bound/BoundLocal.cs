using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundLocal : BoundVariable
    {
        public string Name { get; private set; }

        public BoundLocal(string name, IBoundType type)
            : base(type)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
