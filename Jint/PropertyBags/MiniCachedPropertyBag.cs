using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Jint.Native;

namespace Jint.PropertyBags
{
    internal class MiniCachedPropertyBag : IEnumerable<KeyValuePair<string, Descriptor>>
    {
        private readonly DictionaryPropertyBag _bag;
        private Descriptor _lastAccessed;

        public MiniCachedPropertyBag()
        {
            _bag = new DictionaryPropertyBag();
        }

        public Descriptor Put(string name, Descriptor descriptor)
        {
            _bag.Put(name, descriptor);
            return _lastAccessed = descriptor;
        }

        public void Delete(string name)
        {
            _bag.Delete(name);
            if (_lastAccessed != null && _lastAccessed.Name == name)
                _lastAccessed = null;
        }

        public Descriptor Get(string name)
        {
            if (_lastAccessed != null && _lastAccessed.Name == name)
                return _lastAccessed;
            Descriptor descriptor = _bag.Get(name);
            if (descriptor != null)
                _lastAccessed = descriptor;
            return descriptor;
        }

        public bool TryGet(string name, out Descriptor descriptor)
        {
            if (_lastAccessed != null && _lastAccessed.Name == name)
            {
                descriptor = _lastAccessed;
                return true;
            }
            bool result = _bag.TryGet(name, out descriptor);
            if (result)
                _lastAccessed = descriptor;
            return result;
        }

        public int Count
        {
            get { return _bag.Count; }
        }

        public IEnumerable<Descriptor> Values
        {
            get { return _bag.Values; }
        }

        public IEnumerator<KeyValuePair<string, Descriptor>> GetEnumerator()
        {
            return _bag.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class DictionaryPropertyBag : IEnumerable<KeyValuePair<string, Descriptor>>
        {
            private readonly Dictionary<string, Descriptor> _bag = new Dictionary<string, Descriptor>(5);

            public Descriptor Put(string name, Descriptor descriptor)
            {
                // replace existing without any exception
                _bag[name] = descriptor;
                return descriptor;
            }

            public void Delete(string name)
            {
                _bag.Remove(name);
            }

            public Descriptor Get(string name)
            {
                Descriptor desc;
                TryGet(name, out desc);
                return desc;
            }

            public bool TryGet(string name, out Descriptor descriptor)
            {
                return _bag.TryGetValue(name, out descriptor);
            }

            public int Count
            {
                get { return _bag.Count; }
            }

            public IEnumerable<Descriptor> Values
            {
                get { return _bag.Values; }
            }

            public IEnumerator<KeyValuePair<string, Descriptor>> GetEnumerator()
            {
                return _bag.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
