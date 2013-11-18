using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal class WithScope
    {
        public WithScope Parent { get; private set; }
        public Variable Variable { get; private set; }

        public WithScope(WithScope parent, Variable variable)
        {
            if (variable == null)
                throw new ArgumentNullException("variable");

            Parent = parent;
            Variable = variable;
        }
    }
}
