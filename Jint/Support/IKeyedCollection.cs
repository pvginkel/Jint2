using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Support
{
    public interface IKeyedCollection<in TKey, TItem> : IList<TItem>
    {
        IEqualityComparer<TKey> Comparer { get; }

        TItem this[TKey key] { get; }

        bool Contains(TKey key);

        bool Remove(TKey key);

        bool TryGetValue(TKey key, out TItem item);
    }
}
