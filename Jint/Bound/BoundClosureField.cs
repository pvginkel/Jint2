using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Compiler;

namespace Jint.Bound
{
    internal class BoundClosureField : BoundVariable
    {
        public string Name
        {
            get { return Type.Name; }
        }

        public BoundClosure Closure { get; private set; }
        public IClosureFieldBuilder Builder { get; private set; }

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
            Builder = closure.Builder.CreateClosureFieldBuilder(this);
        }

        public override string ToString()
        {
            return "ClosureField(" + Name + ")";
        }
    }
}
