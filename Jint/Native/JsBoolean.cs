using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public sealed class JsBoolean : JsObject, ILiteral
    {
        private readonly bool _value;

        public override object Value
        {
            get { return _value; }
        }

        public JsBoolean(JsObject prototype)
            : this(false, prototype)
        {
            _value = false;
        }

        public JsBoolean(bool boolean, JsObject prototype)
            : base(prototype)
        {
            _value = boolean;
        }

        public override bool IsClr
        {
            get
            {
                return false;
            }
        }

        public override JsType Type
        {
            get { return JsType.Boolean; }
        }

        public override string Class
        {
            get { return ClassBoolean; }
        }

        public override bool ToBoolean()
        {
            return _value;
        }

        public override string ToString()
        {
            return BooleanToString(_value);
        }

        public static string BooleanToString(bool value)
        {
            return value ? "true" : "false";
        }

        public static double BooleanToNumber(bool value)
        {
            return value ? 1 : 0;
        }

        public override double ToNumber()
        {
            return BooleanToNumber(_value);
        }
    }
}
