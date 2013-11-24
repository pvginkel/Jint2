using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public sealed class JsBoolean : JsInstance, ILiteral
    {
        private readonly bool _value;

        public static readonly JsBoolean True = new JsBoolean(true);
        public static readonly JsBoolean False = new JsBoolean(false);

        public override object Value
        {
            get { return _value; }
            set { throw new InvalidOperationException(); }
        }

        public static JsBoolean Create(bool value)
        {
            return value ? True : False;
        }

        private JsBoolean(bool boolean)
        {
            _value = boolean;
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
            return JsConvert.ToString(_value);
        }

        public override double ToNumber()
        {
            return JsConvert.ToNumber(_value);
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
