using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public sealed class JsDate : JsObject
    {
        internal static long Offset1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
        internal static int TicksFactor = 10000;

        internal JsDate(JsGlobal global, DateTime date, JsObject prototype)
            : base(global, null, prototype, false)
        {
            Value = date;
        }

        internal JsDate(JsGlobal global, double value, JsObject prototype)
            : this(global, CreateDateTime(value), prototype)
        {
        }

        public override string Class
        {
            get { return JsNames.ClassDate; }
        }

        public static DateTime CreateDateTime(double number)
        {
            return new DateTime((long)(number * JsDate.TicksFactor + JsDate.Offset1970), DateTimeKind.Utc);
        }

        public static string Format = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'zzz";
        public static string FormatUtc = "ddd, dd MMM yyyy HH':'mm':'ss 'UTC'";
        public static string DateFormat = "ddd, dd MMM yyyy";
        public static string TimeFormat = "HH':'mm':'ss 'GMT'zzz";
    }
}
