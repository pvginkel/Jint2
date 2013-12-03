using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Jint.Delegates;

namespace Jint.Native
{
    [Serializable]
    public sealed class JsString : JsInstance, ILiteral
    {
        public static readonly JsString Empty = new JsString(String.Empty);

        private readonly string _value;

        public override object Value
        {
            get { return _value; }
            set { throw new InvalidOperationException(); }
        }

        private JsString(string value)
        {
            _value = value;
        }

        public static JsString Create()
        {
            return Empty;
        }

        public static JsString Create(string value)
        {
            if (String.IsNullOrEmpty(value))
                return Empty;

            return new JsString(value);
        }

        public override bool ToBoolean()
        {
            return JsConvert.ToBoolean(_value);
        }

        public override double ToNumber()
        {
            return JsConvert.ToNumber(_value);
        }

        public override string ToString()
        {
            return _value;
        }

        public override string Class
        {
            get { return JsNames.ClassString; }
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

        public override JsInstance ToPrimitive(PreferredType preferredType)
        {
            return this;
        }
    }
}
