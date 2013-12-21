using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Support;

namespace Jint.Bound
{
    internal class BoundClosureFieldCollection : KeyedCollection<string, BoundClosureField>
    {
        protected override string GetKeyForItem(BoundClosureField item)
        {
            return item.Name;
        }
    }
}
