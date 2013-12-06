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

        private readonly JsGlobal _global;

        public JsObject Owner { get; private set; }

        public DictionaryPropertyStore(JsObject owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            Owner = owner;
            _global = Owner.Global;
        }

        public bool HasOwnProperty(JsBox index)
        {
            return HasOwnProperty(_global.ResolveIdentifier(index.ToString()));
        }

        public bool HasOwnProperty(int index)
        {
            Descriptor descriptor;
            return _properties.TryGetValue(index, out descriptor);
        }

        public Descriptor GetOwnDescriptor(JsBox index)
        {
            return GetOwnDescriptor(_global.ResolveIdentifier(index.ToString()));
        }

        public Descriptor GetOwnDescriptor(int index)
        {
            Descriptor result;
            _properties.TryGetValue(index, out result);
            return result;
        }

        public bool Delete(JsBox index)
        {
            return Delete(_global.ResolveIdentifier(index.ToString()));
        }

        public bool Delete(int index)
        {
            Descriptor descriptor;
            if (!Owner.TryGetDescriptor(index, out descriptor))
                return true;

            if (descriptor.Configurable)
            {
                _properties.Remove(index);
                return true;
            }

            return false;

            // TODO: This should throw in strict mode.

            // throw new JintException("Property " + index + " isn't configurable");
        }

        public void DefineOwnProperty(Descriptor currentDescriptor)
        {
            int key = currentDescriptor.Index;
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
                                _properties[key].Set(Owner, currentDescriptor.Get(JsBox.CreateObject(Owner)));
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
                            propertyDescriptor.Set(Owner, currentDescriptor.Get(JsBox.CreateObject(Owner)));
                        }
                        break;
                }
            }
            else
            {
                _properties[key] = currentDescriptor;
            }
        }

        public IEnumerable<int> GetKeys()
        {
            var prototype = Owner.Prototype;

            Debug.Assert(prototype != null);

            if (prototype != _global.PrototypeSink)
            {
                foreach (int key in prototype.GetKeys())
                {
                    if (!HasOwnProperty(key))
                        yield return key;
                }
            }

            foreach (KeyValuePair<int, Descriptor> descriptor in _properties)
            {
                if (descriptor.Value.Enumerable)
                    yield return descriptor.Key;
            }
        }

        public IEnumerable<JsBox> GetValues()
        {
            return
                from descriptor in _properties.Values
                where descriptor.Enumerable
                select descriptor.Get(JsBox.CreateObject(Owner));
        }

        public IEnumerator<KeyValuePair<int, JsBox>> GetEnumerator()
        {
            return (
                from descriptor in _properties
                where descriptor.Value.Enumerable
                select new KeyValuePair<int, JsBox>(descriptor.Key, descriptor.Value.Get(JsBox.CreateObject(Owner)))
            ).GetEnumerator();
        }

        public virtual bool TryGetProperty(JsBox index, out JsBox result)
        {
            var descriptor = Owner.GetDescriptor(index);
            if (descriptor != null)
            {
                result = descriptor.Get(JsBox.CreateObject(Owner));
                return true;
            }

            result = new JsBox();
            return false;
        }

        public virtual bool TryGetProperty(int index, out JsBox result)
        {
            var descriptor = Owner.GetDescriptor(index);
            if (descriptor != null)
            {
                result = descriptor.Get(JsBox.CreateObject(Owner));
                return true;
            }

            result = new JsBox();
            return false;
        }

        public virtual bool TrySetProperty(int index, JsBox value)
        {
            return false;
        }

        public virtual bool TrySetProperty(JsBox index, JsBox value)
        {
            return false;
        }

        private class CachedDictionary : Dictionary<int, Descriptor>
        {
            private Descriptor _lastAccessed;

            public new Descriptor this[int index]
            {
                get
                {
                    if (_lastAccessed != null && _lastAccessed.Index == index)
                        return _lastAccessed;

                    Descriptor descriptor;
                    if (base.TryGetValue(index, out descriptor))
                        _lastAccessed = descriptor;

                    return descriptor;
                }
                set
                {
                    base[index] = value;
                    _lastAccessed = value;
                }
            }

            public new void Remove(int index)
            {
                base.Remove(index);
                if (_lastAccessed != null && _lastAccessed.Index == index)
                    _lastAccessed = null;
            }

            public new bool TryGetValue(int index, out Descriptor descriptor)
            {
                if (_lastAccessed != null && _lastAccessed.Index == index)
                {
                    descriptor = _lastAccessed;
                    return true;
                }

                if (!base.TryGetValue(index, out descriptor))
                    return false;

                _lastAccessed = descriptor;
                return true;
            }
        }
    }
}
