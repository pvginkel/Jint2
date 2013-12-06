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

        public static JsBox Box(double value)
        {
            return JsBox.CreateNumber(value);
        }

        private JsNumber(double value)
        {
            _value = value;
        }

        public override bool ToBoolean()
        {
            return JsConvert.ToBoolean(_value);
        }

        public override double ToNumber()
        {
            return _value;
        }

        public override string ToString()
        {
            return JsConvert.ToString(_value);
        }

        public override object ToObject()
        {
            return _value;
        }

        public override string Class
        {
            get { return JsNames.ClassNumber; }
        }

        public override JsType Type
        {
            get { return JsType.Number; }
        }

        public override bool IsPrimitive
        {
            get { return true; }
        }

        public override JsBox ToPrimitive(PreferredType preferredType)
        {
            return Box((double)Value);
        }
    }
}
