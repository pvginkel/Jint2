using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundClosureField : IBoundWritable, IHasBoundType
    {
        public string Name { get; private set; }
        public BoundClosure Closure { get; private set; }
        public IBoundType Type { get; private set; }

        public BoundClosureField(string name, BoundClosure closure, IBoundType type)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (closure == null)
                throw new ArgumentNullException("closure");
            if (type == null)
                throw new ArgumentNullException("type");

            Name = name;
            Closure = closure;
            Type = type;
        }

        public override string ToString()
        {
            return "ClosureField(" + Name + ")";
        }
    }
}
