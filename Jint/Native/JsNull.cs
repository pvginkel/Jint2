using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class JsNull : JsObject
    {
        public static JsNull Instance = new JsNull();

        private JsNull()
        {
        }

        public override bool IsClr
        {
            get { return false; }
        }

        public override JsType Type
        {
            get { return JsType.Null; }
        }

        public override string Class
        {
            get { return ClassObject; }
        }

        public override int Length
        {
            get
            {
                return 0;
            }
            set { }
        }

        public override bool ToBoolean()
        {
            return false;
        }

        public override double ToNumber()
        {
            return 0d;
        }

        public override string ToString()
        {
            return "null";
        }

        public override JsInstance ToPrimitive(JsGlobal global, PrimitiveHint hint)
        {
            return this;
        }

        public override Descriptor GetDescriptor(string index)
        {
            return null;
        }

        public override IEnumerable<string> GetKeys()
        {
            return new string[0];
        }

        public override object Value
        {
            get { return null; }
            set { }
        }

        public override void DefineOwnProperty(Descriptor value)
        {
        }

        public override bool HasProperty(string key)
        {
            return false;
        }

        public override bool HasOwnProperty(string key)
        {
            return false;
        }

        public override JsInstance this[string index]
        {
            get { return JsUndefined.Instance; }
            set { }
        }
    }
}
