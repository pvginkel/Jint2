using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class JsUndefined : JsInstance
    {
        internal string Name { get; private set; }

        public static JsUndefined Instance = new JsUndefined
        {
            Attributes = PropertyAttributes.DontEnum | PropertyAttributes.DontDelete
        };

        private JsUndefined()
        {
        }

        internal JsUndefined(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Name = name;
        }

        public override string Class
        {
            get { return ClassObject; }
        }

        public override JsType Type
        {
            get { return JsType.Undefined; }
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
            return Double.NaN;
        }

        public override JsInstance ToPrimitive(PreferredType preferredType)
        {
            return this;
        }

        public override object Value
        {
            get { return null; }
            set { }
        }
    }
}
