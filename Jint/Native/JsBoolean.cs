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

        public static JsBox Box(bool value)
        {
            return JsBox.CreateBoolean(value);
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
            get { return JsNames.ClassBoolean; }
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

        public override JsBox ToPrimitive(PreferredType preferredType)
        {
            return _value ? JsBox.True : JsBox.False;
        }
    }
}
