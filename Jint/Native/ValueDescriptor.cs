using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class ValueDescriptor : Descriptor
    {
        public ValueDescriptor(JsObject owner, string name)
            : this(owner, name, PropertyAttributes.None)
        {
        }

        public ValueDescriptor(JsObject owner, string name, PropertyAttributes attributes)
            : base(owner, name, attributes)
        {
        }

        private JsInstance _value;

        public ValueDescriptor(JsObject owner, string name, JsInstance value)
            : this(owner, name, value, PropertyAttributes.None)
        {
        }

        public ValueDescriptor(JsObject owner, string name, JsInstance value, PropertyAttributes attributes)
            : this(owner, name, attributes)
        {
            // Write directly to _value to ignore the writable flag.
            _value = value;
        }

        public override bool IsReference
        {
            get { return false; }
        }

        public override Descriptor Clone()
        {
            return new ValueDescriptor(Owner, Name, _value, Attributes);
        }

        public override JsInstance Get(JsInstance that)
        {
            return _value ?? JsUndefined.Instance;
        }

        public override void Set(JsObject that, JsInstance value)
        {
            if (!Writable)
                throw new JintException("This property is not writable");

            _value = value;
        }

        internal override DescriptorType DescriptorType
        {
            get { return DescriptorType.Value; }
        }
    }
}
