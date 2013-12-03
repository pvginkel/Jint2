using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class PropertyDescriptor : Descriptor
    {
        private readonly JsGlobal _global;

        public JsObject GetFunction { get; internal set; }
        public JsObject SetFunction { get; internal set; }

        public override bool IsReference
        {
            get { return false; }
        }

        public PropertyDescriptor(JsGlobal global, JsObject owner, string name, JsObject getFunction, JsObject setFunction, PropertyAttributes attributes)
            : base(owner, name, attributes)
        {
            GetFunction = getFunction;
            SetFunction = setFunction;
            _global = global;
        }

        public override Descriptor Clone()
        {
            return new PropertyDescriptor(_global, Owner, Name, GetFunction, SetFunction, Attributes);
        }

        public override JsInstance Get(JsInstance that)
        {
            //JsObject that = _global._visitor.CallTarget;
            return _global.Backend.ExecuteFunction(GetFunction, that, JsInstance.EmptyArray, null);
        }

        public override void Set(JsObject that, JsInstance value)
        {
            if (SetFunction == null)
                throw new JsException(JsErrorType.TypeError);

            _global.Backend.ExecuteFunction(SetFunction, that, new[] { value }, null);
        }

        internal override DescriptorType DescriptorType
        {
            get { return DescriptorType.Accessor; }
        }
    }
}
