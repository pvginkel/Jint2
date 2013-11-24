using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Jint.Native
{
    [Serializable]
    public sealed class JsNumber : JsInstance, ILiteral
    {
        public static readonly JsNumber MinValue = new JsNumber(Double.MinValue);
        public static readonly JsNumber MaxValue = new JsNumber(Double.MaxValue);
        public static readonly JsNumber NaN = new JsNumber(Double.NaN);
        public static readonly JsNumber NegativeInfinity = new JsNumber(Double.NegativeInfinity);
        public static readonly JsNumber PositiveInfinity = new JsNumber(Double.PositiveInfinity);

        private readonly double _value;

        public override object Value
        {
            get { return _value; }
            set { throw new InvalidOperationException(); }
        }

        public static JsNumber Create(double value)
        {
            if (Double.IsPositiveInfinity(value))
                return PositiveInfinity;
            if (Double.IsNegativeInfinity(value))
                return NegativeInfinity;
            if (Double.IsNaN(value))
                return NaN;

            return new JsNumber(value);
        }

        private JsNumber(double value)
        {
            _value = value;
        }

        public override bool IsClr
        {
            get { return false; }
        }

        public static bool NumberToBoolean(double value)
        {
            return value != 0 && !Double.IsNaN(value);
        }

        public override bool ToBoolean()
        {
            return NumberToBoolean(_value);
        }

        public override double ToNumber()
        {
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

        public override object ToObject()
        {
            return _value;
        }

        public override string Class
        {
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

        public override JsInstance ToPrimitive(JsGlobal global, PrimitiveHint hint)
        {
            return this;
        }
    }
}
