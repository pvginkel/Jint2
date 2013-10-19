using System;
using System.Collections.Generic;
using System.Text;
using Jint.Native;

namespace Jint
{
    class DoubleListPropertyBag : IPropertyBag
    {
        private readonly IList<string> _keys;
        private readonly IList<Descriptor> _values;

        public DoubleListPropertyBag()
        {
            _keys = new List<string>(5);
            _values = new List<Descriptor>(5);
        }

        #region IPropertyBag Members

        public Descriptor Put(string name, Descriptor descriptor)
        {
            lock (_keys)
            {
                _keys.Add(name);
                _values.Add(descriptor);
            }
            return descriptor;
        }

        public void Delete(string name)
        {
            int index = _keys.IndexOf(name);
            _keys.RemoveAt(index);
            _values.RemoveAt(index);
        }

        public Descriptor Get(string name)
        {
            int index = _keys.IndexOf(name);
            return _values[index];
        }

        public bool TryGet(string name, out Jint.Native.Descriptor descriptor)
        {
            int index = _keys.IndexOf(name);
            if (index < 0)
            {
                descriptor = null;
                return false;
            }
            descriptor = _values[index];
            return true;
        }

        public int Count
        {
            get { return _keys.Count; }
        }

        public IEnumerable<Descriptor> Values
        {
            get { return _values; }
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,Descriptor>> Members

        public IEnumerator<KeyValuePair<string, Descriptor>> GetEnumerator()
        {
            for (int i = 0; i < _keys.Count; i++)
                yield return new KeyValuePair<string, Descriptor>(_keys[i], _values[i]);
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
