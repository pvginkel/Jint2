using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class PropertyDescriptor : Descriptor
    {
        public PropertyDescriptor(JsGlobal global, JsObject owner, string name)
            : base(owner, name)
        {
            _global = global;
            Enumerable = false;
        }

        private readonly JsGlobal _global;

        public JsFunction GetFunction { get; set; }
        public JsFunction SetFunction { get; set; }

        public override bool IsReference
        {
            get { return false; }
        }

        public override Descriptor Clone()
        {
            return new PropertyDescriptor(_global, Owner, Name)
            {
                Enumerable = Enumerable,
                Configurable = Configurable,
                Writable = Writable,
                GetFunction = GetFunction,
                SetFunction = SetFunction
            };
        }

        public override JsInstance Get(JsInstance that)
        {
            //JsObject that = _global._visitor.CallTarget;
            return _global.Backend.ExecuteFunction(GetFunction, that, JsInstance.Empty, null).Result;
        }

        public override void Set(JsObject that, JsInstance value)
        {
            if (SetFunction == null)
                throw new JsException(_global.TypeErrorClass.New());

            _global.Backend.ExecuteFunction(SetFunction, that, new[] { value }, null);
        }

        internal override DescriptorType DescriptorType
        {
            get { return DescriptorType.Accessor; }
        }
    }

    [Serializable]
    public class PropertyDescriptor<T> : PropertyDescriptor
        where T : JsInstance
    {
        public PropertyDescriptor(JsGlobal global, JsObject owner, string name, Func<T, JsInstance> get)
            : base(global, owner, name)
        {
            GetFunction = global.FunctionClass.New(get);
        }

        public PropertyDescriptor(JsGlobal global, JsObject owner, string name, Func<JsGlobal, T, JsInstance> get)
            : base(global, owner, name)
        {
            GetFunction = global.FunctionClass.New(get);
        }

        public PropertyDescriptor(JsGlobal global, JsObject owner, string name, Func<T, JsInstance> get, Func<T, JsInstance[], JsInstance> set)
            : this(global, owner, name, get)
        {
            SetFunction = global.FunctionClass.New(set);
        }

        public PropertyDescriptor(JsGlobal global, JsObject owner, string name, Func<JsGlobal, T, JsInstance> get, Func<T, JsInstance[], JsInstance> set)
            : this(global, owner, name, get)
        {
            SetFunction = global.FunctionClass.New(set);
        }

        public PropertyDescriptor(JsGlobal global, JsObject owner, string name, Func<T, JsInstance> get, Func<JsGlobal, T, JsInstance[], JsInstance> set)
            : this(global, owner, name, get)
        {
            SetFunction = global.FunctionClass.New(set);
        }

        public PropertyDescriptor(JsGlobal global, JsObject owner, string name, Func<JsGlobal, T, JsInstance> get, Func<JsGlobal, T, JsInstance[], JsInstance> set)
            : this(global, owner, name, get)
        {
            SetFunction = global.FunctionClass.New(set);
        }
    }
}
