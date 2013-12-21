using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jint.Support
{
    internal class KeyedCollectionDebugView<K, T>
    {
        private readonly IKeyedCollection<K, T> _container;

        public KeyedCollectionDebugView(IKeyedCollection<K, T> container)
        {
            _container = container;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get { return new List<T>(_container).ToArray(); }
        }
    }
}
