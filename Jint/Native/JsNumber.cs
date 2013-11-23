using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Jint.Native {
    [Serializable]
    public sealed class JsNumber : JsObject, ILiteral {
        private readonly double _value;

        public override object Value {
            get {
                return _value;
            }
        }

        public JsNumber(JsObject prototype)
            : this(0d, prototype) {
        }

        public JsNumber(double num, JsObject prototype)
            : base(prototype) {
            _value = num;
        }

        public JsNumber(int num, JsObject prototype)
            : base(prototype) {
            _value = num;
        }

        public override bool IsClr
        {
            get
            {
                return false;
            }
        }

        public static bool NumberToBoolean(double value) {
            return value != 0 && !Double.IsNaN(value);
        }

        public override bool ToBoolean() {
            return NumberToBoolean(_value);
        }

        public override double ToNumber() {
            return _value;
        }

        public override string ToString()
        {
            return NumberToString(_value);
        }

        public static string NumberToString(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public override object ToObject() {
            return _value;
        }

        public override string Class {
            get { return ClassNumber; }
        }

        public override JsType Type
        {
            get { return JsType.Number; }
        }

        public override bool IsPrimitive
        {
            get { return true; }
        }
    }
}
