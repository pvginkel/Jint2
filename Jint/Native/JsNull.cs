using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public sealed class JsNull : JsInstance
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
            get { return JsNames.ClassObject; }
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

        public override JsBox ToPrimitive(PreferredType preferredType)
        {
            return JsBox.Null;
        }

        public override object Value
        {
            get { return null; }
            set { }
        }
    }
}
