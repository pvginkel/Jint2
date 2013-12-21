using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundClosureField : BoundVariable
    {
        public string Name { get; private set; }
        public BoundClosure Closure { get; private set; }

        public BoundClosureField(string name, BoundClosure closure, IBoundType type)
            : base(type)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (closure == null)
                throw new ArgumentNullException("closure");

            Name = name;
            Closure = closure;
        }

        public override string ToString()
        {
            return "ClosureField(" + Name + ")";
        }
    }
}
