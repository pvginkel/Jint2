using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    public class Variable
    {
        public string Name { get; private set; }
        public VariableType Type { get; set; }
        public bool IsClosedOver { get; set; }

        public Variable(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Name = name;
        }

        public override string ToString()
        {
            return Name + " [" + Type + (IsClosedOver ? "*" : "") + "]";
        }
    }
}
