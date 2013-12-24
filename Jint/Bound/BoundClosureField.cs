using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundClosureField : BoundVariable
    {
        public string Name
        {
            get { return Type.Name; }
        }

        public BoundClosure Closure { get; private set; }

        public override BoundVariableKind Kind
        {
            get { return BoundVariableKind.ClosureField; }
        }

        public BoundClosureField(BoundClosure closure, IBoundType type)
            : base(type)
        {
            if (closure == null)
                throw new ArgumentNullException("closure");

            Closure = closure;
        }

        public override string ToString()
        {
            return "ClosureField(" + Name + ")";
        }
    }
}
