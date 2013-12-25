using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal abstract class BoundLocalBase : BoundVariable
    {
        public string Name
        {
            get { return Type.Name; }
        }

        public bool IsDeclared { get; private set; }

        protected BoundLocalBase(bool isDeclared, IBoundType type)
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
