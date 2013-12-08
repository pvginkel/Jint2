using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal sealed class DictionaryPropertyStore : IPropertyStore
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

        public object GetOwnPropertyRaw(int index)
        {
            Descriptor descriptor;
            if (_properties.TryGetValue(index, out descriptor))
                return descriptor.Value;
            return null;
        }

        public object GetOwnPropertyRaw(JsBox index)
        {
            return GetOwnPropertyRaw(_global.ResolveIdentifier(index.ToString()));
        }

        public void SetPropertyValue(int index, JsBox value)
        {
            // SetPropertyValue is only used to replace the value of a normal,
            // existing, property in the store.

            var descriptor = _properties[index];

            Debug.Assert(!(descriptor.Value is PropertyAccessor));

            if (!descriptor.Writable)
                throw new JintException("This property is not writable");

            _properties[index] = new Descriptor(
                index,
                value.GetValue(),
                _properties[index].Attributes
#if DEBUG
                , _global
#endif
            );
        }

        public void SetPropertyValue(JsBox index, JsBox value)
        {
            SetPropertyValue(_global.ResolveIdentifier(index.ToString()), value);
        }

        public bool DeleteProperty(int index)
        {
            Descriptor descriptor;
            if (!_properties.TryGetValue(index, out descriptor))
                return true;

            if (descriptor.Configurable)
            {
                _properties.Remove(index);
                return true;
            }

            if (_global.HasOption(Options.Strict))
                throw new JintException("Property " + index + " isn't configurable");

            return false;
        }

        public bool DeleteProperty(JsBox index)
        {
            return DeleteProperty(_global.ResolveIdentifier(index.ToString()));
        }

        public void DefineProperty(int index, object value, PropertyAttributes attributes)
        {
            Debug.Assert(!_properties.ContainsKey(index));

            _properties[index] = new Descriptor(
                index,
                value,
                attributes
#if DEBUG
                , _global
#endif
            );
        }

        public void DefineProperty(JsBox index, object value, PropertyAttributes attributes)
        {
            DefineProperty(
                _global.ResolveIdentifier(index.ToString()),
                value,
                attributes
            );
        }

        public IEnumerable<int> GetKeys()
        {
            return _properties.Where(p => p.Value.Enumerable).Select(p => p.Key);
        }

        private class CachedDictionary : Dictionary<int, Descriptor>
        {
            private Descriptor _lastAccessed;

            public new Descriptor this[int index]
            {
                get
                {
                    if (_lastAccessed.IsValid && _lastAccessed.Index == index)
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
                if (_lastAccessed.IsValid && _lastAccessed.Index == index)
                    _lastAccessed = new Descriptor();
            }

            public new bool TryGetValue(int index, out Descriptor descriptor)
            {
                if (_lastAccessed.IsValid && _lastAccessed.Index == index)
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

        [Serializable]
        private struct Descriptor
        {
            // The lower three bits of _index contains the attributes. The rest
            // contains the actual index of the property.

            private readonly int _index;
            private readonly object _value;
#if DEBUG
            private JsGlobal _global;
#endif

            public object Value
            {
                get { return _value; }
            }

            public Descriptor(int index, object value, PropertyAttributes attributes
#if DEBUG
                , JsGlobal global
#endif
            )
            {
                // * 8 for a signed shift.
                _index = index * 8 | (int)attributes;

                Debug.Assert(value != null && value.GetType() != typeof(JsBox));

                _value = value;
#if DEBUG
                _global = global;
#endif
            }

            public bool IsValid
            {
                get { return _value != null; }
            }

            public int Index
            {
                get { return _index >> 3; }
            }

            public PropertyAttributes Attributes
            {
                get { return (PropertyAttributes)(_index & 7); }
            }

            public bool Enumerable
            {
                get { return (Attributes & PropertyAttributes.DontEnum) == 0; }
            }

            public bool Configurable
            {
                get { return (Attributes & PropertyAttributes.DontDelete) == 0; }
            }

            public bool Writable
            {
                get { return (Attributes & PropertyAttributes.ReadOnly) == 0; }
            }

            public override string ToString()
            {
                string value;
                try
                {
                    value = Value.ToString();
                }
                catch
                {
                    value = Value.GetType().FullName;
                }

#if DEBUG
                string name;
                if (Index < 0)
                    name = _global.GetIdentifier(Index);
                else
                    name = "";

                return String.Format(
                    "Name={0}, Index={1}, Value={2}, Attributes={3}",
                    name,
                    Index,
                    value,
                    Attributes
                );
#else
                return String.Format(
                    "Index={0}, Value={1}, Attributes={2}",
                    Index,
                    value,
                    Attributes
                );
#endif
            }
        }
    }
}
