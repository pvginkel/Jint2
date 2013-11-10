using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    public class Variable
    {
        public static readonly Variable This = new Variable(VariableType.This);
        public static readonly Variable Arguments = new Variable(VariableType.Arguments);

        public string Name { get; private set; }
        public VariableType Type { get; set; }
        public ClosedOverVariable ClosureField { get; set; }

        public Variable(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Name = name;
        }

        private Variable(VariableType type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return Name + " [" + Type + (ClosureField != null ? "*" : "") + "]";
        }
    }
}
