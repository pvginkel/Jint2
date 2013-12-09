using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static class DateFunctions
        {
            public static object Now(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return runtime.Global.CreateDate(DateTime.Now);
            }

            internal static object Constructor(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                DateTime result;

                switch (arguments.Length)
                {
                    case 0:
                        result = DateTime.UtcNow;
                        break;

                    case 1:
                        string @class = JsValue.GetClass(arguments[0]);

                        if (
                            (@class == JsNames.ClassNumber || @class == JsNames.ClassObject) &&
                            Double.IsNaN(JsValue.ToNumber(arguments[0]))
                        )
                        {
                            result = JsConvert.ToDateTime(Double.NaN);
                        }
                        else if (@class == JsNames.ClassNumber)
                        {
                            result = JsConvert.ToDateTime(JsValue.ToNumber(arguments[0]));
                        }
                        else
                        {
                            double d;
                            if (ParseDate(runtime.Global, JsValue.ToString(arguments[0]), CultureInfo.InvariantCulture, out d))
                                result = JsConvert.ToDateTime(d);
                            else if (ParseDate(runtime.Global, JsValue.ToString(arguments[0]), CultureInfo.CurrentCulture, out d))
                                result = JsConvert.ToDateTime(d);
                            else
                                result = JsConvert.ToDateTime(0);
                        }
                        break;

                    default:
                        result = new DateTime(0, DateTimeKind.Local);

                        if (arguments.Length > 0)
                        {
                            int year = (int)JsValue.ToNumber(arguments[0]) - 1;
                            if (year < 100)
                                year += 1900;

                            result = result.AddYears(year);
                        }

                        if (arguments.Length > 1)
                            result = result.AddMonths((int)JsValue.ToNumber(arguments[1]));
                        if (arguments.Length > 2)
                            result = result.AddDays((int)JsValue.ToNumber(arguments[2]) - 1);
                        if (arguments.Length > 3)
                            result = result.AddHours((int)JsValue.ToNumber(arguments[3]));
                        if (arguments.Length > 4)
                            result = result.AddMinutes((int)JsValue.ToNumber(arguments[4]));
                        if (arguments.Length > 5)
                            result = result.AddSeconds((int)JsValue.ToNumber(arguments[5]));
                        if (arguments.Length > 6)
                            result = result.AddMilliseconds((int)JsValue.ToNumber(arguments[6]));
                        break;
                }

                var target = (JsObject)@this;
                if (target == runtime.Global.GlobalScope)
                    return ToString(result);

                target.SetClass(JsNames.ClassDate);
                target.IsClr = false;
                target.Value = result;

                return @this;
            }

            private static bool ParseDate(JsGlobal global, string value, IFormatProvider culture, out double result)
            {
                result = 0;

                DateTime date;
                if (DateTime.TryParse(value, culture, DateTimeStyles.None, out date))
                {
                    result = global.CreateDate(date).ToNumber();
                    return true;
                }

                if (DateTime.TryParseExact(value, JsNames.DateTimeFormat, culture, DateTimeStyles.None, out date))
                {
                    result = global.CreateDate(date).ToNumber();
                    return true;
                }

                DateTime ld;

                if (DateTime.TryParseExact(value, JsNames.DateFormat, culture, DateTimeStyles.None, out ld))
                    date = date.AddTicks(ld.Ticks);

                if (DateTime.TryParseExact(value, JsNames.TimeFormat, culture, DateTimeStyles.None, out ld))
                    date = date.AddTicks(ld.Ticks);

                if (date.Ticks > 0)
                {
                    result = global.CreateDate(date).ToNumber();
                    return true;
                }

                return true;
            }

            // 15.9.4.1
            public static object Parse(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                double d;
                if (ParseDate(runtime.Global, JsValue.ToString(arguments[0]), CultureInfo.InvariantCulture, out d))
                    return d;
                else
                    return DoubleBoxes.NaN;
            }

            public static object ParseLocale(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                double d;
                if (ParseDate(runtime.Global, JsValue.ToString(arguments[0]), CultureInfo.CurrentCulture, out d))
                    return d;
                return DoubleBoxes.NaN;
            }

            // 15.9.5.2
            public static object ToString(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return ToString(GetDateTimeValue(@this));
            }

            private static string ToString(DateTime dateTime)
            {
                return dateTime.ToLocalTime().ToString(JsNames.DateTimeFormat, CultureInfo.InvariantCulture);
            }

            public static object ToLocaleString(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return GetDateTimeValue(@this).ToLocalTime().ToString("F", CultureInfo.CurrentCulture);
            }

            // 15.9.5.3
            public static object ToDateString(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return GetDateTimeValue(@this).ToLocalTime().ToString(JsNames.DateFormat, CultureInfo.InvariantCulture);
            }

            // 15.9.5.4
            public static object ToTimeString(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return GetDateTimeValue(@this).ToLocalTime().ToString(JsNames.TimeFormat, CultureInfo.InvariantCulture);
            }

            // 15.9.5.6
            public static object ToLocaleDateString(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return GetDateTimeValue(@this).ToLocalTime().ToString(JsNames.DateFormat);
            }

            // 15.9.5.7
            public static object ToLocaleTimeString(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return GetDateTimeValue(@this).ToLocalTime().ToString(JsNames.TimeFormat);
            }

            // 15.9.5.8
            public static object ValueOf(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return JsConvert.ToNumber(GetDateTimeValue(@this));
            }

            // 15.9.5.9
            public static object GetTime(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return JsConvert.ToNumber(GetDateTimeValue(@this));
            }

            // 15.9.5.10
            public static object GetFullYear(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)GetDateTimeValue(@this).ToLocalTime().Year;
            }

            // 15.9.5.11
            public static object GetUTCFullYear(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)GetDateTimeValue(@this).ToUniversalTime().Year;
            }

            // 15.9.5.12
            public static object GetMonth(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)(GetDateTimeValue(@this).ToLocalTime().Month - 1);
            }

            // 15.9.5.13
            public static object GetUTCMonth(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)(GetDateTimeValue(@this).ToUniversalTime().Month - 1);

            }

            // 15.9.5.14
            public static object GetDate(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)GetDateTimeValue(@this).ToLocalTime().Day;
            }

            // 15.9.5.15
            public static object GetUTCDate(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)GetDateTimeValue(@this).ToUniversalTime().Day;
            }

            // 15.9.5.16
            public static object GetDay(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)((int)GetDateTimeValue(@this).ToLocalTime().DayOfWeek);
            }

            // 15.9.5.17
            public static object GetUTCDay(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)((int)GetDateTimeValue(@this).ToUniversalTime().DayOfWeek);
            }

            // 15.9.5.18
            public static object GetHours(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)GetDateTimeValue(@this).ToLocalTime().Hour;
            }

            // 15.9.5.19
            public static object GetUTCHours(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)GetDateTimeValue(@this).ToUniversalTime().Hour;
            }

            // 15.9.5.20
            public static object GetMinutes(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)GetDateTimeValue(@this).ToLocalTime().Minute;
            }

            // 15.9.5.21
            public static object GetUTCMinutes(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)GetDateTimeValue(@this).ToUniversalTime().Minute;
            }

            // 15.9.5.
            public static object ToUTCString(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return GetDateTimeValue(@this).ToString(JsNames.DateTimeFormatUtc, CultureInfo.InvariantCulture);
            }

            // 15.9.5.22
            public static object GetSeconds(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)GetDateTimeValue(@this).ToLocalTime().Second;
            }

            // 15.9.5.23
            public static object GetUTCSeconds(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)GetDateTimeValue(@this).ToUniversalTime().Second;
            }

            // 15.9.5.24
            public static object GetMilliseconds(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)GetDateTimeValue(@this).ToLocalTime().Millisecond;
            }

            // 15.9.5.25
            public static object GetUTCMilliseconds(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)GetDateTimeValue(@this).ToUniversalTime().Millisecond;
            }

            // 15.9.5.26
            public static object GetTimezoneOffset(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return -TimeZone.CurrentTimeZone.GetUtcOffset(new DateTime()).TotalMinutes;
            }

            // 15.9.5.27
            public static object SetTime(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no time specified");

                ((JsObject)@this).Value = JsConvert.ToNumber(GetDateTimeValue(arguments[0]));

                return @this;
            }

            // 15.9.5.28
            public static object SetMilliseconds(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no millisecond specified");

                var valueOf = GetDateTimeValue(@this).ToLocalTime();
                valueOf = valueOf.AddMilliseconds(-valueOf.Millisecond);
                valueOf = valueOf.AddMilliseconds(JsValue.ToNumber(arguments[0]));

                ((JsObject)@this).Value = runtime.Global.CreateDate(valueOf).ToNumber();

                return @this;
            }

            // 15.9.5.29
            public static object SetUTCMilliseconds(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no millisecond specified");

                var valueOf = GetDateTimeValue(@this);
                valueOf = valueOf.AddMilliseconds(-valueOf.Millisecond);
                valueOf = valueOf.AddMilliseconds(JsValue.ToNumber(arguments[0]));

                ((JsObject)@this).Value = runtime.Global.CreateDate(valueOf).ToNumber();

                return @this;
            }

            // 15.9.5.30
            public static object SetSeconds(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no second specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value).ToLocalTime();
                valueOf = valueOf.AddSeconds(-valueOf.Second);
                valueOf = valueOf.AddSeconds(JsValue.ToNumber(arguments[0]));

                ((JsObject)@this).Value = valueOf;

                if (arguments.Length > 1)
                {
                    var innerParams = new object[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setMilliseconds)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            // 15.9.5.31
            public static object SetUTCSeconds(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no second specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value);
                valueOf = valueOf.AddSeconds(-valueOf.Second);
                valueOf = valueOf.AddSeconds(JsValue.ToNumber(arguments[0]));

                ((JsObject)@this).Value = valueOf;

                if (arguments.Length > 1)
                {
                    object[] innerParams = new object[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setMilliseconds)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            // 15.9.5.33
            public static object SetMinutes(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no minute specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value).ToLocalTime();
                valueOf = valueOf.AddMinutes(-valueOf.Minute);
                valueOf = valueOf.AddMinutes(JsValue.ToNumber(arguments[0]));

                ((JsObject)@this).Value = valueOf;

                if (arguments.Length > 1)
                {
                    object[] innerParams = new object[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setSeconds)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            // 15.9.5.34
            public static object SetUTCMinutes(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no minute specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value);
                valueOf = valueOf.AddMinutes(-valueOf.Minute);
                valueOf = valueOf.AddMinutes(JsValue.ToNumber(arguments[0]));

                ((JsObject)@this).Value = valueOf;

                if (arguments.Length > 1)
                {
                    object[] innerParams = new object[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setSeconds)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            // 15.9.5.35
            public static object SetHours(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no hour specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value).ToLocalTime();
                valueOf = valueOf.AddHours(-valueOf.Hour);
                valueOf = valueOf.AddHours(JsValue.ToNumber(arguments[0]));

                ((JsObject)@this).Value = valueOf;

                if (arguments.Length > 1)
                {
                    object[] innerParams = new object[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setMinutes)).Execute(
                        runtime, @this, innerParams, null
                    );
                }

                return @this;
            }

            // 15.9.5.36
            public static object SetUTCHours(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no hour specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value);
                valueOf = valueOf.AddHours(-valueOf.Hour);
                valueOf = valueOf.AddHours(JsValue.ToNumber(arguments[0]));

                ((JsObject)@this).Value = valueOf;

                if (arguments.Length > 1)
                {
                    object[] innerParams = new object[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setMinutes)).Execute(
                        runtime, @this, innerParams, null
                    );
                }

                return @this;
            }

            // 15.9.5.36
            public static object SetDate(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no date specified");

                var valueOf = GetDateTimeValue(@this).ToLocalTime();
                valueOf = valueOf.AddDays(-valueOf.Day);
                valueOf = valueOf.AddDays(JsValue.ToNumber(arguments[0]));

                ((JsObject)@this).Value = valueOf;

                return @this;

            }

            // 15.9.5.37
            public static object SetUTCDate(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no date specified");

                var valueOf = GetDateTimeValue(@this);
                valueOf = valueOf.AddDays(-valueOf.Day);
                valueOf = valueOf.AddDays(JsValue.ToNumber(arguments[0]));

                ((JsObject)@this).Value = valueOf;

                return @this;
            }

            // 15.9.5.38
            public static object SetMonth(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no month specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value).ToLocalTime();
                valueOf = valueOf.AddMonths(-valueOf.Month);
                valueOf = valueOf.AddMonths((int)JsValue.ToNumber(arguments[0]));

                ((JsObject)@this).Value = valueOf;

                if (arguments.Length > 1)
                {
                    var innerParams = new object[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setDate)).Execute(
                        runtime, @this, innerParams, null
                    );
                }

                return @this;
            }

            // 15.9.5.39
            public static object SetUTCMonth(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no month specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value);
                valueOf = valueOf.AddMonths(-valueOf.Month);
                valueOf = valueOf.AddMonths((int)JsValue.ToNumber(arguments[0]));

                ((JsObject)@this).Value = valueOf;

                if (arguments.Length > 1)
                {
                    var innerParams = new object[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setDate)).Execute(
                        runtime, @this, innerParams, null
                    );
                }

                return @this;
            }

            // 15.9.5.40
            public static object SetFullYear(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no year specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value).ToLocalTime();
                int diff = valueOf.Year - (int)JsValue.ToNumber(arguments[0]);
                valueOf = valueOf.AddYears(-diff);
                target.Value = valueOf;

                if (arguments.Length > 1)
                {
                    var innerParams = new object[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setMonth)).Execute(runtime, @this, innerParams, null);
                }

                return @this;

            }

            // 15.9.5.41
            public static object SetUTCFullYear(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no year specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value);
                valueOf = valueOf.AddYears(-valueOf.Year);
                valueOf = valueOf.AddYears((int)JsValue.ToNumber(arguments[0]));
                target.Value = valueOf;

                if (arguments.Length > 1)
                {
                    var innerParams = new object[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setMonth)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            public static object UTC(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    if (
                        JsValue.IsUndefined(arguments[i]) || (
                            JsValue.GetClass(arguments[i]) == JsNames.ClassNumber && (
                                Double.IsNaN(JsValue.ToNumber(arguments[i])) ||
                                Double.IsInfinity(JsValue.ToNumber(arguments[i]))
                            )
                        )
                    )
                        return DoubleBoxes.NaN;
                }

                var result = runtime.Global.DateClass.Construct(runtime, arguments);
                double offset =
                    JsValue.ToNumber(result) +
                    TimeZone.CurrentTimeZone.GetUtcOffset(new DateTime()).TotalMilliseconds;

                return offset;
            }

            private static DateTime GetDateTimeValue(object value)
            {
                if (value is double)
                    return JsConvert.ToDateTime((double)(value));
                var instance = JsValue.UnwrapValue(value);
                if (instance is double)
                    return JsConvert.ToDateTime((double)instance);

                return (DateTime)instance;
            }
        }
    }
}
