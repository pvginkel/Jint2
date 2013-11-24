using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Jint;
using Jint.Marshal;

namespace Jint.Native
{
    /// <summary>
    /// Descriptor which get and set methods are implemented through delegates
    /// </summary>
    public class NativeDescriptor : Descriptor
    {
        public NativeDescriptor(JsObject owner, string name, JsGetter getter)
            : base(owner, name)
        {
            _getter = getter;
            Writable = false;
        }

        public NativeDescriptor(JsObject owner, string name, JsGetter getter, JsSetter setter)
            : base(owner, name)
        {
            _getter = getter;
            _setter = setter;
        }

        public NativeDescriptor(JsObject owner, NativeDescriptor src)
            : base(owner, src.Name)
        {
            _getter = src._getter;
            _setter = src._setter;
            Writable = src.Writable;
            Configurable = src.Configurable;
            Enumerable = src.Enumerable;
        }

        private readonly JsGetter _getter;
        private readonly JsSetter _setter;

        public override bool IsReference {
            get { return false; }
        }

        public override Descriptor Clone() {
            return new NativeDescriptor(Owner, this);
        }

        public override JsInstance Get(JsInstance that)
        {
            return _getter != null ? _getter((JsObject)that) : JsUndefined.Instance ;
        }

        public override void Set(JsObject that, JsInstance value)
        {
            if (_setter != null)
                _setter(that, value);
        }

        internal override DescriptorType DescriptorType
        {
            get { return DescriptorType.Clr; }
        }


    }
}
