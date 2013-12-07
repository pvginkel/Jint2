using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public sealed class Descriptor
    {
        // The lower three bits of _index contains the attributes. The rest
        // contains the actual index of the property.

        private readonly int _index;
        private object _value;

        public Descriptor(int index, JsBox value, PropertyAttributes attributes)
            : this(index, attributes)
        {
            _value = value.GetValue();
        }

        public Descriptor(int index, JsObject getter, JsObject setter, PropertyAttributes attributes)
            : this(index, attributes)
        {
            _value = new Accessor
            {
                Getter = getter,
                Setter = setter
            };
        }

        private Descriptor(int index, PropertyAttributes attributes)
        {
            // * 8 for a signed shift.

            _index = index * 8 | (int)attributes;
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

        public bool IsAccessor
        {
            get { return _value is Accessor; }
        }

        public JsBox Get(JsBox @this)
        {
            if (_value == null)
                return JsBox.Undefined;

            var accessor = _value as Accessor;
            if (accessor != null)
            {
                return accessor.Getter.Global.ExecuteFunction(
                    accessor.Getter,
                    @this,
                    JsBox.EmptyArray,
                    null
                );
            }

            return JsBox.FromValue(_value);
        }

        public void Set(JsBox @this, JsBox value)
        {
            var accessor = _value as Accessor;
            if (accessor != null)
            {
                if (accessor.Setter == null)
                    throw new JsException(JsErrorType.TypeError);

                accessor.Setter.Global.ExecuteFunction(
                    accessor.Setter,
                    @this,
                    new[] { value },
                    null
                );
            }
            else
            {
                if (!Writable)
                    throw new JintException("This property is not writable");

                _value = value.GetValue();
            }
        }

        public JsObject Getter
        {
            get
            {
                var accessor = _value as Accessor;
                return accessor == null ? null : accessor.Getter;
            }
            set
            {
                ((Accessor)_value).Getter = value;
            }
        }

        public JsObject Setter
        {
            get
            {
                var accessor = _value as Accessor;
                return accessor == null ? null : accessor.Setter;
            }
            set
            {
                ((Accessor)_value).Setter = value;
            }
        }

        private sealed class Accessor
        {
            public JsObject Getter;
            public JsObject Setter;
        }
    }
}
