using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundLocal : BoundVariable
    {
        public string Name { get; private set; }
        public bool IsDeclared { get; private set; }

        public override BoundVariableKind Kind
        {
            get { return BoundVariableKind.Local; }
        }

        public BoundLocal(string name, bool isDeclared, IBoundType type)
            : base(type)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Name = name;
            IsDeclared = isDeclared;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
