using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Jint.Runtime;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static class DateFunctions
        {
            public static JsInstance Now(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return runtime.Global.CreateDate(DateTime.Now);
            }

            internal static JsInstance Constructor(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                JsDate result;

                switch (arguments.Length)
                {
                    case 0:
                        result = runtime.Global.CreateDate(DateTime.UtcNow);
                        break;

                    case 1:
                        if ((arguments[0].Class == JsNames.ClassNumber || arguments[0].Class == JsNames.ClassObject) && double.IsNaN(arguments[0].ToNumber()))
                            return runtime.Global.CreateDate(double.NaN);
                        if (arguments[0].Class == JsNames.ClassNumber)
                            return runtime.Global.CreateDate(arguments[0].ToNumber());

                        double d;
                        if (ParseDate(runtime.Global, arguments[0].ToString(), CultureInfo.InvariantCulture, out d))
                            result = runtime.Global.CreateDate(d);
                        else if (ParseDate(runtime.Global, arguments[0].ToString(), CultureInfo.CurrentCulture, out d))
                            result = runtime.Global.CreateDate(d);
                        else
                            result = runtime.Global.CreateDate(0);
                        break;

                    default:
                        var date = new DateTime(0, DateTimeKind.Local);

                        if (arguments.Length > 0)
                        {
                            int year = (int)arguments[0].ToNumber() - 1;
                            if (year < 100)
                                year += 1900;

                            date = date.AddYears(year);
                        }

                        if (arguments.Length > 1)
                            date = date.AddMonths((int)arguments[1].ToNumber());
                        if (arguments.Length > 2)
                            date = date.AddDays((int)arguments[2].ToNumber() - 1);
                        if (arguments.Length > 3)
                            date = date.AddHours((int)arguments[3].ToNumber());
                        if (arguments.Length > 4)
                            date = date.AddMinutes((int)arguments[4].ToNumber());
                        if (arguments.Length > 5)
                            date = date.AddSeconds((int)arguments[5].ToNumber());
                        if (arguments.Length > 6)
                            date = date.AddMilliseconds((int)arguments[6].ToNumber());

                        result = runtime.Global.CreateDate(date);
                        break;
                }

                if (@this == null || @this == runtime.Global.GlobalScope)
                    return ((JsFunction)result.GetProperty(Id.toString)).Execute(runtime, result, JsInstance.EmptyArray, null);

                return result;
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

                if (DateTime.TryParseExact(value, JsDate.Format, culture, DateTimeStyles.None, out date))
                {
                    result = global.CreateDate(date).ToNumber();
                    return true;
                }

                DateTime ld;

                if (DateTime.TryParseExact(value, JsDate.DateFormat, culture, DateTimeStyles.None, out ld))
                {
                    date = date.AddTicks(ld.Ticks);
                }

                if (DateTime.TryParseExact(value, JsDate.TimeFormat, culture, DateTimeStyles.None, out ld))
                {
                    date = date.AddTicks(ld.Ticks);
                }

                if (date.Ticks > 0)
                {
                    result = global.CreateDate(date).ToNumber();
                    return true;
                }

                return true;
            }

            // 15.9.4.1
            public static JsInstance Parse(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                double d;
                if (ParseDate(runtime.Global, arguments[0].ToString(), CultureInfo.InvariantCulture, out d))
                    return JsNumber.Create(d);
                else
                    return JsNumber.NaN;
            }

            public static JsInstance ParseLocale(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                double d;
                if (ParseDate(runtime.Global, arguments[0].ToString(), CultureInfo.CurrentCulture, out d))
                    return JsNumber.Create(d);
                return JsNumber.NaN;
            }

            // 15.9.5.2
            public static JsInstance ToString(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsString.Create(GetDateTimeValue(@this).ToLocalTime().ToString(JsDate.Format, CultureInfo.InvariantCulture));
            }

            public static JsInstance ToLocaleString(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsString.Create(GetDateTimeValue(@this).ToLocalTime().ToString("F", CultureInfo.CurrentCulture));
            }

            // 15.9.5.3
            public static JsInstance ToDateString(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsString.Create(GetDateTimeValue(@this).ToLocalTime().ToString(JsDate.DateFormat, CultureInfo.InvariantCulture));
            }

            // 15.9.5.4
            public static JsInstance ToTimeString(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsString.Create(GetDateTimeValue(@this).ToLocalTime().ToString(JsDate.TimeFormat, CultureInfo.InvariantCulture));
            }

            // 15.9.5.6
            public static JsInstance ToLocaleDateString(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsString.Create(GetDateTimeValue(@this).ToLocalTime().ToString(JsDate.DateFormat));
            }

            // 15.9.5.7
            public static JsInstance ToLocaleTimeString(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsString.Create(GetDateTimeValue(@this).ToLocalTime().ToString(JsDate.TimeFormat));
            }

            // 15.9.5.8
            public static JsInstance ValueOf(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(JsConvert.ToNumber(GetDateTimeValue(@this)));
            }

            // 15.9.5.9
            public static JsInstance GetTime(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(JsConvert.ToNumber(GetDateTimeValue(@this)));
            }

            // 15.9.5.10
            public static JsInstance GetFullYear(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(GetDateTimeValue(@this).ToLocalTime().Year);
            }

            // 15.9.5.11
            public static JsInstance GetUTCFullYear(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(GetDateTimeValue(@this).ToUniversalTime().Year);
            }

            // 15.9.5.12
            public static JsInstance GetMonth(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(GetDateTimeValue(@this).ToLocalTime().Month - 1);
            }

            // 15.9.5.13
            public static JsInstance GetUTCMonth(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(GetDateTimeValue(@this).ToUniversalTime().Month - 1);

            }

            // 15.9.5.14
            public static JsInstance GetDate(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(GetDateTimeValue(@this).ToLocalTime().Day);
            }

            // 15.9.5.15
            public static JsInstance GetUTCDate(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(GetDateTimeValue(@this).ToUniversalTime().Day);
            }

            // 15.9.5.16
            public static JsInstance GetDay(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create((int)GetDateTimeValue(@this).ToLocalTime().DayOfWeek);
            }

            // 15.9.5.17
            public static JsInstance GetUTCDay(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create((int)GetDateTimeValue(@this).ToUniversalTime().DayOfWeek);
            }

            // 15.9.5.18
            public static JsInstance GetHours(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(GetDateTimeValue(@this).ToLocalTime().Hour);
            }

            // 15.9.5.19
            public static JsInstance GetUTCHours(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(GetDateTimeValue(@this).ToUniversalTime().Hour);
            }

            // 15.9.5.20
            public static JsInstance GetMinutes(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(GetDateTimeValue(@this).ToLocalTime().Minute);
            }

            // 15.9.5.21
            public static JsInstance GetUTCMinutes(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(GetDateTimeValue(@this).ToUniversalTime().Minute);
            }

            // 15.9.5.
            public static JsInstance ToUTCString(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsString.Create(GetDateTimeValue(@this).ToString(JsDate.FormatUtc, CultureInfo.InvariantCulture));
            }

            // 15.9.5.22
            public static JsInstance GetSeconds(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(GetDateTimeValue(@this).ToLocalTime().Second);
            }

            // 15.9.5.23
            public static JsInstance GetUTCSeconds(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(GetDateTimeValue(@this).ToUniversalTime().Second);
            }

            // 15.9.5.24
            public static JsInstance GetMilliseconds(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(GetDateTimeValue(@this).ToLocalTime().Millisecond);
            }

            // 15.9.5.25
            public static JsInstance GetUTCMilliseconds(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(GetDateTimeValue(@this).ToUniversalTime().Millisecond);
            }

            // 15.9.5.26
            public static JsInstance GetTimezoneOffset(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(-TimeZone.CurrentTimeZone.GetUtcOffset(new DateTime()).TotalMinutes);
            }

            // 15.9.5.27
            public static JsInstance SetTime(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no time specified");

                @this.Value = JsConvert.ToNumber(GetDateTimeValue(arguments[0]));

                return @this;
            }

            // 15.9.5.28
            public static JsInstance SetMilliseconds(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no millisecond specified");

                var valueOf = GetDateTimeValue(@this).ToLocalTime();
                valueOf = valueOf.AddMilliseconds(-valueOf.Millisecond);
                valueOf = valueOf.AddMilliseconds(arguments[0].ToNumber());
                @this.Value = runtime.Global.CreateDate(valueOf).ToNumber();
                return @this;
            }

            // 15.9.5.29
            public static JsInstance SetUTCMilliseconds(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no millisecond specified");

                var valueOf = GetDateTimeValue(@this);
                valueOf = valueOf.AddMilliseconds(-valueOf.Millisecond);
                valueOf = valueOf.AddMilliseconds(arguments[0].ToNumber());
                @this.Value = runtime.Global.CreateDate(valueOf).ToNumber();
                return @this;
            }

            // 15.9.5.30
            public static JsInstance SetSeconds(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no second specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value).ToLocalTime();
                valueOf = valueOf.AddSeconds(-valueOf.Second);
                valueOf = valueOf.AddSeconds(arguments[0].ToNumber());
                @this.Value = valueOf;
                if (arguments.Length > 1)
                {
                    var innerParams = new JsInstance[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsFunction)target.GetProperty(Id.setMilliseconds)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            // 15.9.5.31
            public static JsInstance SetUTCSeconds(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no second specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value);
                valueOf = valueOf.AddSeconds(-valueOf.Second);
                valueOf = valueOf.AddSeconds(arguments[0].ToNumber());
                @this.Value = valueOf;

                if (arguments.Length > 1)
                {
                    JsInstance[] innerParams = new JsInstance[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsFunction)target.GetProperty(Id.setMilliseconds)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            // 15.9.5.33
            public static JsInstance SetMinutes(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no minute specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value).ToLocalTime();
                valueOf = valueOf.AddMinutes(-valueOf.Minute);
                valueOf = valueOf.AddMinutes(arguments[0].ToNumber());
                @this.Value = valueOf;

                if (arguments.Length > 1)
                {
                    JsInstance[] innerParams = new JsInstance[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsFunction)target.GetProperty(Id.setSeconds)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            // 15.9.5.34
            public static JsInstance SetUTCMinutes(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no minute specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value);
                valueOf = valueOf.AddMinutes(-valueOf.Minute);
                valueOf = valueOf.AddMinutes(arguments[0].ToNumber());
                @this.Value = valueOf;

                if (arguments.Length > 1)
                {
                    JsInstance[] innerParams = new JsInstance[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsFunction)target.GetProperty(Id.setSeconds)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            // 15.9.5.35
            public static JsInstance SetHours(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no hour specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value).ToLocalTime();
                valueOf = valueOf.AddHours(-valueOf.Hour);
                valueOf = valueOf.AddHours(arguments[0].ToNumber());
                @this.Value = valueOf;

                if (arguments.Length > 1)
                {
                    JsInstance[] innerParams = new JsInstance[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsFunction)target.GetProperty(Id.setMinutes)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            // 15.9.5.36
            public static JsInstance SetUTCHours(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no hour specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value);
                valueOf = valueOf.AddHours(-valueOf.Hour);
                valueOf = valueOf.AddHours(arguments[0].ToNumber());
                @this.Value = valueOf;

                if (arguments.Length > 1)
                {
                    JsInstance[] innerParams = new JsInstance[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsFunction)target.GetProperty(Id.setMinutes)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            // 15.9.5.36
            public static JsInstance SetDate(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no date specified");

                var valueOf = GetDateTimeValue(@this).ToLocalTime();
                valueOf = valueOf.AddDays(-valueOf.Day);
                valueOf = valueOf.AddDays(arguments[0].ToNumber());
                @this.Value = valueOf;

                return @this;

            }

            // 15.9.5.37
            public static JsInstance SetUTCDate(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no date specified");

                var valueOf = GetDateTimeValue(@this);
                valueOf = valueOf.AddDays(-valueOf.Day);
                valueOf = valueOf.AddDays(arguments[0].ToNumber());
                @this.Value = valueOf;

                return @this;
            }

            // 15.9.5.38
            public static JsInstance SetMonth(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no month specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value).ToLocalTime();
                valueOf = valueOf.AddMonths(-valueOf.Month);
                valueOf = valueOf.AddMonths((int)arguments[0].ToNumber());
                @this.Value = valueOf;

                if (arguments.Length > 1)
                {
                    var innerParams = new JsInstance[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsFunction)target.GetProperty(Id.setDate)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            // 15.9.5.39
            public static JsInstance SetUTCMonth(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                    throw new ArgumentException("There was no month specified");

                var target = (JsObject)@this;
                var valueOf = ((DateTime)target.Value);
                valueOf = valueOf.AddMonths(-valueOf.Month);
                valueOf = valueOf.AddMonths((int)arguments[0].ToNumber());
                @this.Value = valueOf;

                if (arguments.Length > 1)
                {
                    var innerParams = new JsInstance[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsFunction)target.GetProperty(Id.setDate)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            // 15.9.5.40
            public static JsInstance SetFullYear(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
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
                    var innerParams = new JsInstance[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsFunction)target.GetProperty(Id.setMonth)).Execute(runtime, @this, innerParams, null);
                }

                return @this;

            }

            // 15.9.5.41
            public static JsInstance SetUTCFullYear(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
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
                    var innerParams = new JsInstance[arguments.Length - 1];
                    Array.Copy(arguments, 1, innerParams, 0, innerParams.Length);
                    @this = ((JsFunction)target.GetProperty(Id.setMonth)).Execute(runtime, @this, innerParams, null);
                }

                return @this;
            }

            public static JsInstance UTC(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    if (
                        JsInstance.IsUndefined(arguments[i]) || (
                            arguments[i].Class == JsNames.ClassNumber && (
                                Double.IsNaN(arguments[i].ToNumber()) ||
                                Double.IsInfinity(arguments[i].ToNumber())
                            )
                        )
                    )
                        return JsNumber.NaN;
                }

                var result = runtime.Global.DateClass.Construct(runtime, arguments);
                double offset =
                    result.ToNumber() +
                    TimeZone.CurrentTimeZone.GetUtcOffset(new DateTime()).TotalMilliseconds;

                return JsNumber.Create(offset);
            }

            private static DateTime GetDateTimeValue(JsInstance value)
            {
                if (value.Type == JsType.Number)
                    return JsDate.CreateDateTime(value.ToNumber());
                if (value.Value is double)
                    return JsDate.CreateDateTime((double)value.Value);

                return (DateTime)value.Value;
            }
        }
    }
}
