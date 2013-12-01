using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal class DictionaryPropertyStore : IPropertyStore
    {
        private readonly CachedDictionary _properties = new CachedDictionary();

        public JsObject Owner { get; private set; }

        public DictionaryPropertyStore(JsObject owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            Owner = owner;
        }

        public void SetLength(int length)
        {
        }

        public bool HasOwnProperty(JsInstance index)
        {
            return HasOwnProperty(index.ToString());
        }

        public bool HasOwnProperty(string index)
        {
            Descriptor descriptor;
            return _properties.TryGetValue(index, out descriptor);
        }

        public Descriptor GetOwnDescriptor(JsInstance index)
        {
            return GetOwnDescriptor(index.ToString());
        }

        public Descriptor GetOwnDescriptor(string index)
        {
            Descriptor result;
            _properties.TryGetValue(index, out result);
            return result;
        }

        public bool Delete(JsInstance index)
        {
            return Delete(index.ToString());
        }

        public bool Delete(string index)
        {
            Descriptor descriptor;
            if (!Owner.TryGetDescriptor(index, out descriptor))
                return true;

            if (descriptor.Configurable)
            {
                _properties.Remove(index);
                Owner.Length--;
                return true;
            }

            return false;

            // TODO: This should throw in strict mode.

            // throw new JintException("Property " + index + " isn't configurable");
        }

        public void DefineOwnProperty(Descriptor currentDescriptor)
        {
            string key = currentDescriptor.Name;
            Descriptor descriptor;
            if (_properties.TryGetValue(key, out descriptor))
            {
                // Updating an existing property.

                switch (descriptor.DescriptorType)
                {
                    case DescriptorType.Value:
                        switch (currentDescriptor.DescriptorType)
                        {
                            case DescriptorType.Value:
                                _properties[key].Set(Owner, currentDescriptor.Get(Owner));
                                break;

                            case DescriptorType.Accessor:
                                _properties.Remove(key);
                                _properties[key] = currentDescriptor;
                                break;

                            case DescriptorType.Clr:
                                throw new NotSupportedException();
                        }
                        break;

                    case DescriptorType.Accessor:
                        var propertyDescriptor = (PropertyDescriptor)descriptor;
                        var currentPropertyDescriptor = (PropertyDescriptor)currentDescriptor;

                        if (currentDescriptor.DescriptorType == DescriptorType.Accessor)
                        {
                            if (currentPropertyDescriptor.GetFunction != null)
                                propertyDescriptor.GetFunction = currentPropertyDescriptor.GetFunction;
                            if (currentPropertyDescriptor.SetFunction != null)
                                propertyDescriptor.SetFunction = currentPropertyDescriptor.SetFunction;
                        }
                        else
                        {
                            propertyDescriptor.Set(Owner, currentDescriptor.Get(Owner));
                        }
                        break;
                }
            }
            else
            {
                _properties[key] = currentDescriptor;
                Owner.Length++;
            }
        }

        public IEnumerable<string> GetKeys()
        {
            var prototype = Owner.Prototype;

            Debug.Assert(prototype != null);

            if (prototype != Owner.Global.PrototypeSink)
            {
                foreach (string key in prototype.GetKeys())
                {
                    if (!HasOwnProperty(key))
                        yield return key;
                }
            }

            foreach (KeyValuePair<string, Descriptor> descriptor in _properties)
            {
                if (descriptor.Value.Enumerable)
                    yield return descriptor.Key;
            }
        }

        public IEnumerable<JsInstance> GetValues()
        {
            return
                from descriptor in _properties.Values
                where descriptor.Enumerable
                select descriptor.Get(Owner);
        }

        public IEnumerator<KeyValuePair<string, JsInstance>> GetEnumerator()
        {
            return (
                from descriptor in _properties
                where descriptor.Value.Enumerable
                select new KeyValuePair<string, JsInstance>(descriptor.Key, descriptor.Value.Get(Owner))
            ).GetEnumerator();
        }

        public virtual bool TryGetProperty(JsInstance index, out JsInstance result)
        {
            var descriptor = Owner.GetDescriptor(index);
            if (descriptor != null)
            {
                result = descriptor.Get(Owner);
                return true;
            }

            result = null;
            return false;
        }

        public virtual bool TryGetProperty(string index, out JsInstance result)
        {
            var descriptor = Owner.GetDescriptor(index);
            if (descriptor != null)
            {
                result = descriptor.Get(Owner);
                return true;
            }

            result = null;
            return false;
        }

        public virtual bool TrySetProperty(string index, JsInstance value)
        {
            return false;
        }

        public virtual bool TrySetProperty(JsInstance index, JsInstance value)
        {
            return false;
        }

        private class CachedDictionary : Dictionary<string, Descriptor>
        {
            private Descriptor _lastAccessed;

            public new Descriptor this[string name]
            {
                get
                {
                    if (_lastAccessed != null && _lastAccessed.Name == name)
                        return _lastAccessed;

                    Descriptor descriptor;
                    if (base.TryGetValue(name, out descriptor))
                        _lastAccessed = descriptor;

                    return descriptor;
                }
                set
                {
                    base[name] = value;
                    _lastAccessed = value;
                }
            }

            public new void Remove(string name)
            {
                base.Remove(name);
                if (_lastAccessed != null && _lastAccessed.Name == name)
                    _lastAccessed = null;
            }

            public new bool TryGetValue(string name, out Descriptor descriptor)
            {
                if (_lastAccessed != null && _lastAccessed.Name == name)
                {
                    descriptor = _lastAccessed;
                    return true;
                }

                if (!base.TryGetValue(name, out descriptor))
                    return false;

                _lastAccessed = descriptor;
                return true;
            }
        }
    }
}
