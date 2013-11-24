using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    public static class JsConvert
    {
        public static bool ToBoolean(string value)
        {
            if (value == null)
                return false;
            if (value == "true" || value.Length > 0)
                return true;

            return false;
        }

        public static bool ToBoolean(double value)
        {
            return value != 0 && !Double.IsNaN(value);
        }

        public static string ToString(bool value)
        {
            return value ? "true" : "false";
        }

        public static string ToString(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static double ToNumber(string value)
        {
            // 9.3.1

            if (String.IsNullOrWhiteSpace(value))
                return 0;

            double result;

            if (Double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                return result;

            return Double.NaN;
        }

        public static double ToNumber(bool value)
        {
            return value ? 1 : 0;
        }

        public static string ToString(DateTime value)
        {
            return value.ToLocalTime().ToString(JsDate.Format, CultureInfo.InvariantCulture);
        }

        public static double ToNumber(DateTime value)
        {
            return (value.ToUniversalTime().Ticks - JsDate.Offset1970) / JsDate.TicksFactor;
        }
    }
}
