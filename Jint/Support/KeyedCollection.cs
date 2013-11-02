using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Support
{
    public abstract class KeyedCollection<TKey, TItem> : System.Collections.ObjectModel.KeyedCollection<TKey, TItem>
    {
        public bool TryGetItem(TKey key, out TItem item)
        {
            if (Contains(key))
            {
                item = this[key];
                return true;
            }

            item = default(TItem);
            return false;
        }
    }
}
