using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Support;

namespace Jint.Expressions
{
    internal class VariableCollection : KeyedCollection<string, Variable>
    {
        protected override string GetKeyForItem(Variable item)
        {
            return item.Name;
        }

        public Variable AddOrGet(string variableName)
        {
            return AddOrGet(variableName, -1);
        }

        public Variable AddOrGet(string variableName, int index)
        {
            if (variableName == null)
                throw new ArgumentNullException("variableName");

            Variable variable;
            if (!TryGetItem(variableName, out variable))
            {
                variable = new Variable(variableName, index);
                Add(variable);
            }

            return variable;
        }
    }
}
