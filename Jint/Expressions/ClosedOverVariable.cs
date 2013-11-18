using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Jint.Expressions
{
    internal class ClosedOverVariable
    {
        public Closure Closure { get; private set; }
        public FieldInfo Field { get; private set; }
        public Variable Variable { get; private set; }

        public ClosedOverVariable(Closure closure, Variable variable, FieldInfo field)
        {
            if (closure == null)
                throw new ArgumentNullException("closure");
            if (variable == null)
                throw new ArgumentNullException("variable");
            if (field == null)
                throw new ArgumentNullException("field");

            Closure = closure;
            Variable = variable;
            Field = field;
        }
    }
}
