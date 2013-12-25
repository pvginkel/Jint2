using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Jint.Compiler
{
    internal class ClosureParentField
    {
        public IClosureBuilder Closure { get; private set; }
        public FieldInfo Field { get; private set; }

        public ClosureParentField(IClosureBuilder closure, FieldInfo field)
        {
            if (closure == null)
                throw new ArgumentNullException("closure");
            if (field == null)
                throw new ArgumentNullException("field");

            Closure = closure;
            Field = field;
        }
    }
}
