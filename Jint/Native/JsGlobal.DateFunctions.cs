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
            public static JsBox Now(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsBox.CreateObject(runtime.Global.CreateDate(DateTime.Now));
            }

            internal static JsBox Constructor(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                DateTime result;

                switch (arguments.Length)
                {
                    case 0:
                        result = DateTime.UtcNow;
                        break;

                    case 1:
                        string @class = arguments[0].GetClass();

                        if (
                            (@class == JsNames.ClassNumber || @class == JsNames.ClassObject) &&
                            Double.IsNaN(arguments[0].ToNumber())
                        )
                        {
                            result = JsConvert.ToDateTime(Double.NaN);
                        }
                        else if (@class == JsNames.ClassNumber)
                        {
                            result = JsConvert.ToDateTime(arguments[0].ToNumber());
                        }
                        else
                        {
                            double d;
                            if (ParseDate(runtime.Global, arguments[0].ToString(), CultureInfo.InvariantCulture, out d))
                                result = JsConvert.ToDateTime(d);
                            else if (ParseDate(runtime.Global, arguments[0].ToString(), CultureInfo.CurrentCulture, out d))
                                result = JsConvert.ToDateTime(d);
                            else
                                result = JsConvert.ToDateTime(0);
                        }
                        break;

                    default:
                        result = new DateTime(0, DateTimeKind.Local);

                        if (arguments.Length > 0)
                        {
                            int year = (int)arguments[0].ToNumber() - 1;
                            if (year < 100)
                                year += 1900;

                            result = result.AddYears(year);
                        }

                        if (arguments.Length > 1)
                            result = result.AddMonths((int)arguments[1].ToNumber());
                        if (arguments.Length > 2)
                            result = result.AddDays((int)arguments[2].ToNumber() - 1);
                        if (arguments.Length > 3)
                            result = result.AddHours((int)arguments[3].ToNumber());
                        if (arguments.Length > 4)
                            result = result.AddMinutes((int)arguments[4].ToNumber());
                        if (arguments.Length > 5)
                            result = result.AddSeconds((int)arguments[5].ToNumber());
                        if (arguments.Length > 6)
                            result = result.AddMilliseconds((int)arguments[6].ToNumber());
                        break;
                }

                var target = (JsObject)@this;
                if (target == runtime.Global.GlobalScope)
                    return JsString.Box(ToString(result));

                target.SetClass(JsNames.ClassDate);
                target.SetIsClr(false);
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
            public static JsBox Parse(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                double d;
                if (ParseDate(runtime.Global, arguments[0].ToString(), CultureInfo.InvariantCulture, out d))
                    return JsNumber.Box(d);
                else
                    return JsBox.NaN;
            }

            public static JsBox ParseLocale(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                double d;
                if (ParseDate(runtime.Global, arguments[0].ToString(), CultureInfo.CurrentCulture, out d))
                    return JsNumber.Box(d);
                return JsBox.NaN;
            }

            // 15.9.5.2
            public static JsBox ToString(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsString.Box(ToString(GetDateTimeValue(@this)));
            }

            private static string ToString(DateTime dateTime)
            {
                return dateTime.ToLocalTime().ToString(JsNames.DateTimeFormat, CultureInfo.InvariantCulture);
            }

            public static JsBox ToLocaleString(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsString.Box(GetDateTimeValue(@this).ToLocalTime().ToString("F", CultureInfo.CurrentCulture));
            }

            // 15.9.5.3
            public static JsBox ToDateString(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsString.Box(GetDateTimeValue(@this).ToLocalTime().ToString(JsNames.DateFormat, CultureInfo.InvariantCulture));
            }

            // 15.9.5.4
            public static JsBox ToTimeString(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsString.Box(GetDateTimeValue(@this).ToLocalTime().ToString(JsNames.TimeFormat, CultureInfo.InvariantCulture));
            }

            // 15.9.5.6
            public static JsBox ToLocaleDateString(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsString.Box(GetDateTimeValue(@this).ToLocalTime().ToString(JsNames.DateFormat));
            }

            // 15.9.5.7
            public static JsBox ToLocaleTimeString(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsString.Box(GetDateTimeValue(@this).ToLocalTime().ToString(JsNames.TimeFormat));
            }

            // 15.9.5.8
            public static JsBox ValueOf(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(JsConvert.ToNumber(GetDateTimeValue(@this)));
            }

            // 15.9.5.9
            public static JsBox GetTime(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(JsConvert.ToNumber(GetDateTimeValue(@this)));
            }

            // 15.9.5.10
            public static JsBox GetFullYear(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(GetDateTimeValue(@this).ToLocalTime().Year);
            }

            // 15.9.5.11
            public static JsBox GetUTCFullYear(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(GetDateTimeValue(@this).ToUniversalTime().Year);
            }

            // 15.9.5.12
            public static JsBox GetMonth(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(GetDateTimeValue(@this).ToLocalTime().Month - 1);
            }

            // 15.9.5.13
            public static JsBox GetUTCMonth(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(GetDateTimeValue(@this).ToUniversalTime().Month - 1);

            }

            // 15.9.5.14
            public static JsBox GetDate(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(GetDateTimeValue(@this).ToLocalTime().Day);
            }

            // 15.9.5.15
            public static JsBox GetUTCDate(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(GetDateTimeValue(@this).ToUniversalTime().Day);
            }

            // 15.9.5.16
            public static JsBox GetDay(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box((int)GetDateTimeValue(@this).ToLocalTime().DayOfWeek);
            }

            // 15.9.5.17
            public static JsBox GetUTCDay(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box((int)GetDateTimeValue(@this).ToUniversalTime().DayOfWeek);
            }

            // 15.9.5.18
            public static JsBox GetHours(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(GetDateTimeValue(@this).ToLocalTime().Hour);
            }

            // 15.9.5.19
            public static JsBox GetUTCHours(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(GetDateTimeValue(@this).ToUniversalTime().Hour);
            }

            // 15.9.5.20
            public static JsBox GetMinutes(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(GetDateTimeValue(@this).ToLocalTime().Minute);
            }

            // 15.9.5.21
            public static JsBox GetUTCMinutes(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(GetDateTimeValue(@this).ToUniversalTime().Minute);
            }

            // 15.9.5.
            public static JsBox ToUTCString(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsString.Box(GetDateTimeValue(@this).ToString(JsNames.DateTimeFormatUtc, CultureInfo.InvariantCulture));
            }

            // 15.9.5.22
            public static JsBox GetSeconds(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(GetDateTimeValue(@this).ToLocalTime().Second);
            }

            // 15.9.5.23
            public static JsBox GetUTCSeconds(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(GetDateTimeValue(@this).ToUniversalTime().Second);
            }

            // 15.9.5.24
            public static JsBox GetMilliseconds(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(GetDateTimeValue(@this).ToLocalTime().Millisecond);
            }

            // 15.9.5.25
            public static JsBox GetUTCMilliseconds(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(GetDateTimeValue(@this).ToUniversalTime().Millisecond);
            }

            // 15.9.5.26
            public static JsBox GetTimezoneOffset(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(-TimeZone.CurrentTimeZone.GetUtcOffset(new DateTime()).TotalMinutes);
            }

            // 15.9.5.27
            public static JsBox SetTime(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no time specified");

                ((JsObject)@this).Value = JsConvert.ToNumber(GetDateTimeValue(arguments[0]));

                return @this;
            }

            // 15.9.5.28
            public static JsBox SetMilliseconds(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no millisecond specified");

                var valueOf = GetDateTimeValue(@this).ToLocalTime();
                valueOf = valueOf.AddMilliseconds(-valueOf.Millisecond);
                valueOf = valueOf.AddMilliseconds(arguments[0].ToNumber());

                ((JsObject)@this).Value = runtime.Global.CreateDate(valueOf).ToNumber();

                return @this;
            }

            // 15.9.5.29
            public static JsBox SetUTCMilliseconds(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no millisecond specified");

                var valueOf = GetDateTimeValue(@this);
                valueOf = valueOf.AddMilliseconds(-valueOf.Millisecond);
                valueOf = valueOf.AddMilliseconds(arguments[0].ToNumber());

                ((JsObject)@this).Value = runtime.Global.CreateDate(valueOf).ToNumber();

                return @this;
            }

            // 15.9.5.30
            public static JsBox SetSeconds(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no second specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value).ToLocalTime();
                valueOf = valueOf.AddSeconds(-valueOf.Second);
                valueOf = valueOf.AddSeconds(arguments[0].ToNumber());

                ((JsObject)@this).Value = valueOf;

                if (arguments.Length > 1)
                {
                    var innerParams = new JsBox[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setMilliseconds)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            // 15.9.5.31
            public static JsBox SetUTCSeconds(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no second specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value);
                valueOf = valueOf.AddSeconds(-valueOf.Second);
                valueOf = valueOf.AddSeconds(arguments[0].ToNumber());

                ((JsObject)@this).Value = valueOf;

                if (arguments.Length > 1)
                {
                    JsBox[] innerParams = new JsBox[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setMilliseconds)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            // 15.9.5.33
            public static JsBox SetMinutes(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no minute specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value).ToLocalTime();
                valueOf = valueOf.AddMinutes(-valueOf.Minute);
                valueOf = valueOf.AddMinutes(arguments[0].ToNumber());

                ((JsObject)@this).Value = valueOf;

                if (arguments.Length > 1)
                {
                    JsBox[] innerParams = new JsBox[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setSeconds)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            // 15.9.5.34
            public static JsBox SetUTCMinutes(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no minute specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value);
                valueOf = valueOf.AddMinutes(-valueOf.Minute);
                valueOf = valueOf.AddMinutes(arguments[0].ToNumber());

                ((JsObject)@this).Value = valueOf;

                if (arguments.Length > 1)
                {
                    JsBox[] innerParams = new JsBox[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setSeconds)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            // 15.9.5.35
            public static JsBox SetHours(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no hour specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value).ToLocalTime();
                valueOf = valueOf.AddHours(-valueOf.Hour);
                valueOf = valueOf.AddHours(arguments[0].ToNumber());

                ((JsObject)@this).Value = valueOf;

                if (arguments.Length > 1)
                {
                    JsBox[] innerParams = new JsBox[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setMinutes)).Execute(
                        runtime, @this, innerParams, null
                    );
                }

                return @this;
            }

            // 15.9.5.36
            public static JsBox SetUTCHours(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no hour specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value);
                valueOf = valueOf.AddHours(-valueOf.Hour);
                valueOf = valueOf.AddHours(arguments[0].ToNumber());

                ((JsObject)@this).Value = valueOf;

                if (arguments.Length > 1)
                {
                    JsBox[] innerParams = new JsBox[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setMinutes)).Execute(
                        runtime, @this, innerParams, null
                    );
                }

                return @this;
            }

            // 15.9.5.36
            public static JsBox SetDate(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no date specified");

                var valueOf = GetDateTimeValue(@this).ToLocalTime();
                valueOf = valueOf.AddDays(-valueOf.Day);
                valueOf = valueOf.AddDays(arguments[0].ToNumber());

                ((JsObject)@this).Value = valueOf;

                return @this;

            }

            // 15.9.5.37
            public static JsBox SetUTCDate(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no date specified");

                var valueOf = GetDateTimeValue(@this);
                valueOf = valueOf.AddDays(-valueOf.Day);
                valueOf = valueOf.AddDays(arguments[0].ToNumber());

                ((JsObject)@this).Value = valueOf;

                return @this;
            }

            // 15.9.5.38
            public static JsBox SetMonth(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no month specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value).ToLocalTime();
                valueOf = valueOf.AddMonths(-valueOf.Month);
                valueOf = valueOf.AddMonths((int)arguments[0].ToNumber());

                ((JsObject)@this).Value = valueOf;

                if (arguments.Length > 1)
                {
                    var innerParams = new JsBox[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setDate)).Execute(
                        runtime, @this, innerParams, null
                    );
                }

                return @this;
            }

            // 15.9.5.39
            public static JsBox SetUTCMonth(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no month specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value);
                valueOf = valueOf.AddMonths(-valueOf.Month);
                valueOf = valueOf.AddMonths((int)arguments[0].ToNumber());

                ((JsObject)@this).Value = valueOf;

                if (arguments.Length > 1)
                {
                    var innerParams = new JsBox[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setDate)).Execute(
                        runtime, @this, innerParams, null
                    );
                }

                return @this;
            }

            // 15.9.5.40
            public static JsBox SetFullYear(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no year specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value).ToLocalTime();
                int diff = valueOf.Year - (int)arguments[0].ToNumber();
                valueOf = valueOf.AddYears(-diff);
                target.Value = valueOf;

                if (arguments.Length > 1)
                {
                    var innerParams = new JsBox[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setMonth)).Execute(runtime, @this, innerParams, null);
                }

                return @this;

            }

            // 15.9.5.41
            public static JsBox SetUTCFullYear(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no year specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value);
                valueOf = valueOf.AddYears(-valueOf.Year);
                valueOf = valueOf.AddYears((int)arguments[0].ToNumber());
                target.Value = valueOf;

                if (arguments.Length > 1)
                {
                    var innerParams = new JsBox[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsObject)target.GetProperty(Id.setMonth)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            public static JsBox UTC(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    if (
                        arguments[i].IsUndefined || (
                            arguments[i].GetClass() == JsNames.ClassNumber && (
                                Double.IsNaN(arguments[i].ToNumber()) ||
                                Double.IsInfinity(arguments[i].ToNumber())
                            )
                        )
                    )
                        return JsBox.NaN;
                }

                var result = runtime.Global.DateClass.Construct(runtime, arguments);
                double offset =
                    result.ToNumber() +
                    TimeZone.CurrentTimeZone.GetUtcOffset(new DateTime()).TotalMilliseconds;

                return JsNumber.Box(offset);
            }

            private static DateTime GetDateTimeValue(JsBox value)
            {
                if (value.IsNumber)
                    return JsConvert.ToDateTime(value.ToNumber());
                var instance = value.ToInstance();
                if (instance.Value is double)
                    return JsConvert.ToDateTime((double)instance.Value);

                return (DateTime)instance.Value;
            }
        }
    }
}
