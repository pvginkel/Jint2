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

        public object GetOwnPropertyRaw(object index)
        {
            // Overload resolution prefers this overload instead of
            // FastPropertyStore.GetOwnPropertyRaw(int). Because of this, references
            // to the DictionaryPropertyStore that need that overload, need to
            // cast the reference to FastPropertyStore.

            Debug.Assert(!(index is int));

            return base.GetOwnPropertyRaw(_global.ResolveIdentifier(JsValue.ToString(index)));
        }

        public void SetPropertyValue(int index, object value)
        {
            // SetPropertyValue is only used to replace the value of a normal,
            // existing, property in the store. GetAttributes throws when the
            // entry is not in the set.

            Debug.Assert(!(base.GetOwnPropertyRaw(index) is PropertyAccessor));

            var attributes = GetAttributes(index);
            if ((attributes & PropertyAttributes.ReadOnly) != 0)
                throw new JintException("This property is not writable");

            SetValue(index, value);
        }

        public void SetPropertyValue(object index, object value)
        {
            SetPropertyValue(_global.ResolveIdentifier(JsValue.ToString(index)), value);
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

        public bool DeleteProperty(object index)
        {
            return DeleteProperty(_global.ResolveIdentifier(JsValue.ToString(index)));
        }

        public void DefineProperty(int index, object value, PropertyAttributes attributes)
        {
            Debug.Assert(base.GetOwnPropertyRaw(index) == null);

            Add(
                index,
                attributes,
                value
            );
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
            return GetKeys(true);
        }
    }
}
