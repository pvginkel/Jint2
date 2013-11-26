using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native.Interop
{
    /// <summary>
    /// Descriptor which get and set methods are implemented through delegates
    /// </summary>
    public class NativeDescriptor : Descriptor
    {
        public NativeDescriptor(JsObject owner, string name, WrappedGetter getter)
            : this(owner, name, getter, PropertyAttributes.ReadOnly)
        {
        }

        public NativeDescriptor(JsObject owner, string name, WrappedGetter getter, PropertyAttributes attributes)
            : base(owner, name, attributes)
        {
            _getter = getter;
        }

        public NativeDescriptor(JsObject owner, string name, WrappedGetter getter, WrappedSetter setter)
            : this(owner, name, getter, setter, PropertyAttributes.None)
        {
        }

        public NativeDescriptor(JsObject owner, string name, WrappedGetter getter, WrappedSetter setter, PropertyAttributes attributes)
            : base(owner, name, attributes)
        {
            _getter = getter;
            _setter = setter;
        }

        public NativeDescriptor(JsObject owner, NativeDescriptor other)
            : base(owner, other.Name, other.Attributes)
        {
            _getter = other._getter;
            _setter = other._setter;
        }

        private readonly WrappedGetter _getter;
        private readonly WrappedSetter _setter;

        public override bool IsReference
        {
            get { return false; }
        }

        public override Descriptor Clone()
        {
            return new NativeDescriptor(Owner, this);
        }

        public override JsInstance Get(JsInstance that)
        {
            return
                _getter != null
                ? _getter(Owner.Global, (JsObject)that)
                : JsUndefined.Instance;
        }

        public override void Set(JsObject that, JsInstance value)
        {
            if (_setter != null)
                _setter(Owner.Global, that, value);
        }

        internal override DescriptorType DescriptorType
        {
            get { return DescriptorType.Clr; }
        }
    }
}
