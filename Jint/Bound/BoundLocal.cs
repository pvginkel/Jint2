using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundLocal : BoundVariable
    {
        public string Name
        {
            get { return Type.Name; }
        }

        public bool IsDeclared { get; private set; }

        public override BoundVariableKind Kind
        {
            get { return BoundVariableKind.Local; }
        }

        public BoundLocal(bool isDeclared, IBoundType type)
            : base(type)
        {
            IsDeclared = isDeclared;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
