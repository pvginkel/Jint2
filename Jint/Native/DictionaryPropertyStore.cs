using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Support;

namespace Jint.Native
{
    internal sealed class DictionaryPropertyStore : FastPropertyStore, IPropertyStore
    {
        private readonly JsGlobal _global;

        public JsObject Owner { get; private set; }

        public DictionaryPropertyStore(JsObject owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            Owner = owner;
            _global = Owner.Global;
        }

        public object GetOwnPropertyRaw(JsBox index)
        {
            return GetOwnPropertyRaw(_global.ResolveIdentifier(index.ToString()));
        }

        public void SetPropertyValue(int index, JsBox value)
        {
            // SetPropertyValue is only used to replace the value of a normal,
            // existing, property in the store. GetAttributes throws when the
            // entry is not in the set.

            Debug.Assert(!(GetOwnPropertyRaw(index) is PropertyAccessor));

            var attributes = GetAttributes(index);
            if ((attributes & PropertyAttributes.ReadOnly) != 0)
                throw new JintException("This property is not writable");

            SetValue(index, value.GetValue());
        }

        public void SetPropertyValue(JsBox index, JsBox value)
        {
            SetPropertyValue(_global.ResolveIdentifier(index.ToString()), value);
        }

        public bool DeleteProperty(int index)
        {
            PropertyAttributes attributes;
            if (!TryGetAttributes(index, out attributes))
                return true;

            if ((attributes & PropertyAttributes.DontDelete) == 0)
            {
                Remove(index);
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
            Debug.Assert(GetOwnPropertyRaw(index) == null);

            Add(
                index,
                attributes,
                value
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
            return GetKeys(true);
        }
    }
}
