using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal abstract class BoundExpression : BoundNode
    {
        public abstract BoundValueType ValueType { get; }
    }
}
