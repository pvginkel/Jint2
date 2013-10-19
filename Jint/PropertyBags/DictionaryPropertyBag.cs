using System;
using System.Collections.Generic;
using System.Text;
using Jint.Native;

namespace Jint
{
    class DictionaryPropertyBag : IPropertyBag
    {
        private readonly Dictionary<string, Descriptor> _bag = new Dictionary<string, Descriptor>(5);

        #region IPropertyBag Members

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

        public Jint.Native.Descriptor Get(string name)
        {
            Descriptor desc;
            TryGet(name, out desc);
            return desc;
        }

        public bool TryGet(string name, out Jint.Native.Descriptor descriptor)
        {
           return _bag.TryGetValue(name, out descriptor);
        }

        public int Count
        {
            get { return _bag.Count; }
        }

        #endregion

        #region IPropertyBag Members


        public IEnumerable<Descriptor> Values
        {
            get { return _bag.Values; }
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,Descriptor>> Members

        public IEnumerator<KeyValuePair<string, Descriptor>> GetEnumerator()
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
