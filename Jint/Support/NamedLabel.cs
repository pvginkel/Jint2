using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace Jint.Support
{
    internal class NamedLabel
    {
        public string Name { get; private set; }
        public Label Label { get; private set; }

        public NamedLabel(string name, Label label)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Name = name;
            Label = label;
        }
    }
}
