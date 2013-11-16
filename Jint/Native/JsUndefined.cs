using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class JsUndefined : JsObject
    {
        private readonly IGlobal _global;
        internal string Name { get; private set; }

        public static JsUndefined Instance = new JsUndefined
        {
            Attributes = PropertyAttributes.DontEnum | PropertyAttributes.DontDelete
        };

        public JsUndefined()
        {
        }

        internal JsUndefined(IGlobal global, string name)
        {
            if (global == null)
                throw new ArgumentNullException("global");
            if (name == null)
                throw new ArgumentNullException("name");

            _global = global;
            Name = name;
        }

        public override int Length
        {
            get { return 0; }
            set { }
        }

        public override bool IsClr
        {
            get { return false; }
        }

        public override Descriptor GetDescriptor(string index)
        {
            return null;
        }

        public override string Class
        {
            get { return ClassObject; }
        }

        public override string Type
        {
            get
            {
                return TypeUndefined;
            }
        }

        public override string ToString()
        {
            return "undefined";
        }

        public override object ToObject()
        {
            return null;
        }

        public override bool ToBoolean()
        {
            return false;
        }

        public override double ToNumber()
        {
            return double.NaN;
        }

        public override JsInstance this[string index]
        {
            get
            {
                if (Name != null)
                    return _global.Backend.ResolveUndefined(Name + "." + index, null);

                return base[index];
            }
            set
            {
                base[index] = value;
            }
        }
    }
}
