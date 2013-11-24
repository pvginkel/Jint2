using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class JsNull : JsInstance
    {
        public static JsNull Instance = new JsNull();

        private JsNull()
        {
        }

        public override JsType Type
        {
            get { return JsType.Null; }
        }

        public override string Class
        {
            get { return ClassObject; }
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

        public override JsInstance ToPrimitive(PrimitiveHint hint)
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
