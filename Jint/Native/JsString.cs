using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Jint.Delegates;

namespace Jint.Native
{
    [Serializable]
    public sealed class JsString : JsObject, ILiteral
    {
        private readonly string _value;

        public override object Value
        {
            get
            {
                return _value;
            }
        }
        public JsString(JsObject prototype)
            : base(prototype)
        {
            _value = String.Empty;
        }

        public JsString(string str, JsObject prototype)
            : base(prototype)
        {
            _value = str;
        }

        public static bool StringToBoolean(string value)
        {
            if (value == null)
                return false;
            if (value == "true" || value.Length > 0)
            {
                return true;
            }

            return false;
        }

        public override bool IsClr
        {
            get
            {
                return false;
            }
        }

        public override bool ToBoolean()
        {
            return StringToBoolean(_value);
        }

        public static double StringToNumber(string value)
        {
            if (value == null)
            {
                return double.NaN;
            }

            double result;

            if (Double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                return result;

            return Double.NaN;
        }

        public override double ToNumber()
        {
            return StringToNumber(_value);
        }

        public override string ToSource()
        {
            /// TODO: subsitute escape sequences
            return _value == null ? "null" : "'" + ToString() + "'";
        }

        public override string ToString()
        {
            return _value;
        }

        public override string Class
        {
            get { return ClassString; }
        }

        public override JsType Type
        {
            get { return JsType.String; }
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override bool IsPrimitive
        {
            get { return true; }
        }
    }
}
