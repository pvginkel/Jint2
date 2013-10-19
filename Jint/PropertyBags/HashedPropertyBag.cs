using System;
using System.Collections.Generic;
using System.Text;
using Jint.Native;
using System.Collections;

namespace Jint.PropertyBags
{
    public class HashedPropertyBag : IPropertyBag
    {
        public HashedPropertyBag()
        {
            _keys = new Hashtable();
        }

        private readonly Hashtable _keys;

        #region IPropertyBag Members

        public Jint.Native.Descriptor Put(string name, Jint.Native.Descriptor descriptor)
        {
            _keys.Add(name, descriptor);
            return descriptor;
        }

        public void Delete(string name)
        {
            _keys.Remove(name);
        }

        public Jint.Native.Descriptor Get(string name)
        {
            return _keys[name] as Descriptor;
        }

        public bool TryGet(string name, out Jint.Native.Descriptor descriptor)
        {
            descriptor = Get(name);
            return descriptor != null;
        }

        public int Count
        {
            get { return _keys.Count; }
        }

        public IEnumerable<Jint.Native.Descriptor> Values
        {
            get
            {
                foreach (DictionaryEntry de in _keys)
                {
                    yield return de.Value as Descriptor;
                }
            }
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,Descriptor>> Members

        public IEnumerator<KeyValuePair<string, Jint.Native.Descriptor>> GetEnumerator()
        {
            foreach (DictionaryEntry de in _keys)
            {
                yield return new KeyValuePair<string, Descriptor>(de.Key as string, de.Value as Descriptor);
            }
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
