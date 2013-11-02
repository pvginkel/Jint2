using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Support;

namespace Jint.Expressions
{
    public class VariableCollection : KeyedCollection<string, Variable>
    {
        protected override string GetKeyForItem(Variable item)
        {
            return item.Name;
        }
    }
}
