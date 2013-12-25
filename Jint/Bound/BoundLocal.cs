using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundLocal : BoundLocalBase
    {
        public override BoundVariableKind Kind
        {
            get { return BoundVariableKind.Local; }
        }

        public BoundLocal(bool isDeclared, IBoundType type)
            : base(isDeclared, type)
        {
        }
    }
}
