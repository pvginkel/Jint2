using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Jint.Native;

namespace Jint.PropertyBags
{
    public class MiniCachedPropertyBag : IPropertyBag
    {
        private readonly IPropertyBag _bag;
        private Descriptor _lastAccessed;

        public MiniCachedPropertyBag()
        {
            _bag = new DictionaryPropertyBag();
        }

        #region IPropertyBag Members

        public Jint.Native.Descriptor Put(string name, Jint.Native.Descriptor descriptor)
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

        public Jint.Native.Descriptor Get(string name)
        {
            if (_lastAccessed != null && _lastAccessed.Name == name)
                return _lastAccessed;
            Descriptor descriptor = _bag.Get(name);
            if (descriptor != null)
                _lastAccessed = descriptor;
            return descriptor;
        }

        public bool TryGet(string name, out Jint.Native.Descriptor descriptor)
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

        public IEnumerable<Jint.Native.Descriptor> Values
        {
            get { return _bag.Values; }
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,Descriptor>> Members

        public IEnumerator<KeyValuePair<string, Jint.Native.Descriptor>> GetEnumerator()
        {
            return _bag.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
