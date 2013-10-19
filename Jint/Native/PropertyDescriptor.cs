using System;
using System.Collections.Generic;
using System.Text;
using Jint.Delegates;

namespace Jint.Native {
    [Serializable]
    public class PropertyDescriptor : Descriptor {
        public PropertyDescriptor(IGlobal global, JsDictionaryObject owner, string name)
            : base(owner, name) {
            _global = global;
            Enumerable = false;
        }

        private readonly IGlobal _global;

        public JsFunction GetFunction { get; set; }
        public JsFunction SetFunction { get; set; }

        public override bool IsReference {
            get { return false; }
        }

        public override Descriptor Clone() {
            return new PropertyDescriptor(_global, Owner, Name) {
                Enumerable = Enumerable,
                Configurable = Configurable,
                Writable = Writable,
                GetFunction = GetFunction,
                SetFunction = SetFunction
            };
        }

        public override JsInstance Get(JsDictionaryObject that) {
            //JsDictionaryObject that = _global._visitor.CallTarget;
            _global.Visitor.ExecuteFunction(GetFunction, that, JsInstance.Empty);
            return _global.Visitor.Returned;
        }

        public override void Set(JsDictionaryObject that, JsInstance value) {
            if (SetFunction == null)
                throw new JsException(_global.TypeErrorClass.New());
            //JsDictionaryObject that = _global._visitor.CallTarget;
            _global.Visitor.ExecuteFunction(SetFunction, that, new JsInstance[] { value });
        }

        internal override DescriptorType DescriptorType {
            get { return DescriptorType.Accessor; }
        }
    }

    [Serializable]
    public class PropertyDescriptor<T> : PropertyDescriptor
        where T : JsInstance {
        public PropertyDescriptor(IGlobal global, JsDictionaryObject owner, string name, Func<T, JsInstance> get)
            : base(global, owner, name) {
            GetFunction = global.FunctionClass.New<T>(get);
        }

        public PropertyDescriptor(IGlobal global, JsDictionaryObject owner, string name, Func<T, JsInstance> get, Func<T, JsInstance[], JsInstance> set)
            : this(global, owner, name, get) {
            SetFunction = global.FunctionClass.New<T>(set);
        }
    }
}
