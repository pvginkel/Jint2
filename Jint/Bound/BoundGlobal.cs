using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundGlobal : BoundLocalBase
    {
        public override BoundVariableKind Kind
        {
            get { return BoundVariableKind.Global; }
        }

        public BoundGlobal(bool isDeclared, IBoundType type)
            : base(isDeclared, type)
        {
        }

        public override string ToString()
        {
            return base.ToString() + " (Global)";
        }
    }
}
