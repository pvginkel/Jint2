using System;
using System.Collections.Generic;
using System.Text;
using Jint.Delegates;
using System.Globalization;

namespace Jint.Native
{
    [Serializable]
    public sealed class JsDate : JsObject
    {
        internal static long Offset1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
        internal static int TicksFactor = 10000;

        private DateTime _value;

        public override object Value
        {
            get { return _value; }
            set
            {
                if (value is DateTime)
                    _value = (DateTime)value;
                else if (value is double)
                    _value = JsDateConstructor.CreateDateTime((double)value);
            }
        }

        public JsDate(JsGlobal global, JsObject prototype)
            : base(global, prototype)
        {
            _value = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        public JsDate(JsGlobal global, DateTime date, JsObject prototype)
            : base(global, prototype)
        {
            _value = date;
        }

        public JsDate(JsGlobal global, double value, JsObject prototype)
            : this(global, JsDateConstructor.CreateDateTime(value), prototype)
        {
        }

        public override double ToNumber()
        {
            return DateToDouble(_value);
        }

        public static string Format = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'zzz";
        public static string FormatUtc = "ddd, dd MMM yyyy HH':'mm':'ss 'UTC'";
        public static string DateFormat = "ddd, dd MMM yyyy";
        public static string TimeFormat = "HH':'mm':'ss 'GMT'zzz";

        public static double DateToDouble(DateTime date)
        {
            return (date.ToUniversalTime().Ticks - Offset1970) / TicksFactor;
        }

        public static string DateToString(DateTime date)
        {
            return date.ToLocalTime().ToString(Format, CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            return DateToString(_value);
        }

        public override object ToObject()
        {
            return _value;
        }

        public override string Class
        {
            get { return ClassDate; }
        }
    }
}
