using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Support;

namespace Jint.Native
{
    [DebuggerTypeProxy(typeof(DictionaryPropertyStoreDebugView))]
    internal sealed class DictionaryPropertyStore : IPropertyStore
    {
        private readonly JsGlobal _global;
        private object[] _values = new object[JsSchema.InitialArraySize];

        public JsObject Owner { get; private set; }
        public JsSchema Schema { get; private set; }

        public DictionaryPropertyStore(JsObject owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            Owner = owner;
            _global = Owner.Global;
            Schema = _global.RootSchema;
        }

        public object GetOwnPropertyRaw(int index)
        {
            int offset = Schema.GetOffset(index);
            if (offset < 0)
                return null;
            return _values[offset];
        }

        public object GetOwnPropertyRaw(int index, ref DictionaryCacheSlot cacheSlot)
        {
            int offset = Schema.GetOffset(index);
            if (offset < 0)
                return null;

            cacheSlot = new DictionaryCacheSlot(Schema, offset);

            return _values[offset];
        }

        public object GetOwnPropertyRaw(object index)
        {
            return GetOwnPropertyRaw(_global.ResolveIdentifier(JsValue.ToString(index)));
        }

        public object GetOwnPropertyRawUnchecked(int offset)
        {
            return _values[offset];
        }

        public void SetPropertyValue(int index, object value)
        {
            // SetPropertyValue is only used to replace the value of a normal,
            // existing, property in the store. GetAttributes throws when the
            // entry is not in the set.

            Debug.Assert(!(GetOwnPropertyRaw(index) is PropertyAccessor));

            var attributes = Schema.GetAttributes(index);
            if ((attributes & PropertyAttributes.ReadOnly) != 0)
                throw new JintException("This property is not writable");

            _values[Schema.GetOffset(index)] = value;
        }

        public void SetPropertyValue(object index, object value)
        {
            SetPropertyValue(_global.ResolveIdentifier(JsValue.ToString(index)), value);
        }

        public void SetPropertyValueUnchecked(int offset, object value)
        {
            _values[offset] = value;
        }

        public bool DeleteProperty(int index, bool strict)
        {
            PropertyAttributes attributes;
            if (!Schema.TryGetAttributes(index, out attributes))
                return true;

            if ((attributes & PropertyAttributes.DontDelete) == 0)
            {
                Schema = Schema.Remove(index, ref _values);
                return true;
            }

            if (strict)
                throw new JsException(JsErrorType.TypeError, "Property " + index + " isn't configurable");

            return false;
        }

        public bool DeleteProperty(object index, bool strict)
        {
            return DeleteProperty(_global.ResolveIdentifier(JsValue.ToString(index)), strict);
        }

        public void DefineProperty(int index, object value, PropertyAttributes attributes)
        {
            Debug.Assert(GetOwnPropertyRaw(index) == null);

            Schema = Schema.Add(index, attributes, ref _values, value);
        }

        public void DefineProperty(object index, object value, PropertyAttributes attributes)
        {
            DefineProperty(
                _global.ResolveIdentifier(JsValue.ToString(index)),
                value,
                attributes
            );
        }

        public IEnumerable<int> GetKeys()
        {
            return Schema.GetKeys(true);
        }

        internal class DictionaryPropertyStoreDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly DictionaryPropertyStore _container;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            private DisplayEntry[] Items
            {
                get
                {
                    var entries = new List<DisplayEntry>();

                    foreach (int key in _container.Schema.GetKeys(false))
                    {
                        entries.Add(new DisplayEntry(
                            _container.Owner.Global.GetIdentifier(key),
                            _container._values[_container.Schema.GetOffset(key)],
                            _container.Schema.GetAttributes(key)
                        ));
                    }

                    entries.Sort((a, b) => a.Key.CompareTo(b.Key));

                    return entries.ToArray();
                }
            }

            public DictionaryPropertyStoreDebugView(DictionaryPropertyStore container)
            {
                _container = container;
            }

            [DebuggerDisplay("Key={Key}, Value={Value}, Attributes={Attributes}")]
            private class DisplayEntry
            {
                public string Key { get; private set; }
                public object Value { get; private set; }
                public PropertyAttributes Attributes { get; private set; }

                public DisplayEntry(string key, object value, PropertyAttributes attributes)
                {
                    Key = key;
                    Value = value;
                    Attributes = attributes;
                }
            }
        }
    }
}
