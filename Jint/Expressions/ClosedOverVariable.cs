using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal class ClosedOverVariable
    {
        public Closure Closure { get; private set; }
        public Variable Variable { get; private set; }

        public ClosedOverVariable(Closure closure, Variable variable)
        {
            if (closure == null)
                throw new ArgumentNullException("closure");
            if (variable == null)
                throw new ArgumentNullException("variable");

            Closure = closure;
            Variable = variable;
        }
    }
}
