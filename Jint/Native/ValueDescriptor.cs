using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class ValueDescriptor : Descriptor
    {
        private JsBox _value;

        public override bool IsReference
        {
            get { return false; }
        }

        public ValueDescriptor(JsObject owner, string name)
            : this(owner, name, PropertyAttributes.None)
        {
        }

        public ValueDescriptor(JsObject owner, string name, PropertyAttributes attributes)
            : base(owner, name, attributes)
        {
        }

        public ValueDescriptor(JsObject owner, string name, JsBox value)
            : this(owner, name, value, PropertyAttributes.None)
        {
        }

        public ValueDescriptor(JsObject owner, string name, JsBox value, PropertyAttributes attributes)
            : this(owner, name, attributes)
        {
            // Write directly to _value to ignore the writable flag.
            _value = value;
        }

        public override Descriptor Clone()
        {
            return new ValueDescriptor(Owner, Name, _value, Attributes);
        }

        public override JsBox Get(JsBox that)
        {
            if (_value.IsValid)
                return _value;

            return JsBox.Undefined;
        }

        public override void Set(JsObject that, JsBox value)
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
