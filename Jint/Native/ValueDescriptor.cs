using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native {
    [Serializable]
    public class ValueDescriptor : Descriptor {
        public ValueDescriptor(JsDictionaryObject owner, string name)
            : base(owner, name) {
            Enumerable = true;
            Writable = true;
            Configurable = true;
        }

        private JsInstance _value;

        public ValueDescriptor(JsDictionaryObject owner, string name, JsInstance value)
            : this(owner, name) {
            Set(null, value);
        }

        public override bool IsReference {
            get { return false; }
        }

        public override Descriptor Clone() {
            return new ValueDescriptor(Owner, Name, _value) {
                Enumerable = Enumerable,
                Configurable = Configurable,
                Writable = Writable
            };
        }

        public override JsInstance Get(JsDictionaryObject that) {
            return _value ?? JsUndefined.Instance;
        }

        public override void Set(JsDictionaryObject that, JsInstance value) {
            if (!Writable)
                throw new JintException("This property is not writable");
            _value = value;
        }

        internal override DescriptorType DescriptorType {
            get { return DescriptorType.Value; }
        }
    }
}
