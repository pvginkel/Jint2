using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Jint.Native
{
    [Serializable]
    public class JsDateConstructor : JsConstructor
    {
        protected JsDateConstructor(JsGlobal global, bool initializeUTC)
            : base(global, BuildPrototype(global))
        {
            Name = "Date";

            DefineOwnProperty("now", new ClrFunction(new Func<JsDate>(() => { return Global.DateClass.New(DateTime.Now); }), global.FunctionClass.Prototype), PropertyAttributes.DontEnum);
            DefineOwnProperty("parse", new JsFunctionWrapper(ParseImpl, global.FunctionClass.Prototype), PropertyAttributes.DontEnum);
            DefineOwnProperty("parseLocale", new JsFunctionWrapper(ParseLocaleImpl, global.FunctionClass.Prototype), PropertyAttributes.DontEnum);
            DefineOwnProperty("UTC", new JsFunctionWrapper(UTCImpl, global.FunctionClass.Prototype), PropertyAttributes.DontEnum);
        }

        private static JsObject BuildPrototype(JsGlobal global)
        {
            var prototype = new JsObject(global.FunctionClass.Prototype);

            prototype.DefineOwnProperty("UTC", new JsFunctionWrapper(UTCImpl, global.FunctionClass.Prototype), PropertyAttributes.DontEnum);

            #region Static Methods
            prototype.DefineOwnProperty("now", new ClrFunction(new Func<JsDate>(() => { return global.DateClass.New(DateTime.Now); }), global.FunctionClass.Prototype), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("parse", new JsFunctionWrapper(ParseImpl, global.FunctionClass.Prototype), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("parseLocale", new JsFunctionWrapper(ParseLocaleImpl, global.FunctionClass.Prototype), PropertyAttributes.DontEnum);
            #endregion

            #region Methods
            prototype.DefineOwnProperty("toString", global.FunctionClass.New<JsDictionaryObject>(ToStringImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toDateString", global.FunctionClass.New<JsDictionaryObject>(ToDateStringImpl, 0), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toTimeString", global.FunctionClass.New<JsDictionaryObject>(ToTimeStringImpl, 0), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toLocaleString", global.FunctionClass.New<JsDictionaryObject>(ToLocaleStringImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toLocaleDateString", global.FunctionClass.New<JsDictionaryObject>(ToLocaleDateStringImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toLocaleTimeString", global.FunctionClass.New<JsDictionaryObject>(ToLocaleTimeStringImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("valueOf", global.FunctionClass.New<JsDictionaryObject>(ValueOfImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("getTime", global.FunctionClass.New<JsDictionaryObject>(GetTimeImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("getFullYear", global.FunctionClass.New<JsDictionaryObject>(GetFullYearImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("getUTCFullYear", global.FunctionClass.New<JsDictionaryObject>(GetUTCFullYearImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("getMonth", global.FunctionClass.New<JsDictionaryObject>(GetMonthImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("getUTCMonth", global.FunctionClass.New<JsDictionaryObject>(GetUTCMonthImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("getDate", global.FunctionClass.New<JsDictionaryObject>(GetDateImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("getUTCDate", global.FunctionClass.New<JsDictionaryObject>(GetUTCDateImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("getDay", global.FunctionClass.New<JsDictionaryObject>(GetDayImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("getUTCDay", global.FunctionClass.New<JsDictionaryObject>(GetUTCDayImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("getHours", global.FunctionClass.New<JsDictionaryObject>(GetHoursImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("getUTCHours", global.FunctionClass.New<JsDictionaryObject>(GetUTCHoursImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("getMinutes", global.FunctionClass.New<JsDictionaryObject>(GetMinutesImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("getUTCMinutes", global.FunctionClass.New<JsDictionaryObject>(GetUTCMinutesImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("getSeconds", global.FunctionClass.New<JsDictionaryObject>(GetSecondsImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("getUTCSeconds", global.FunctionClass.New<JsDictionaryObject>(GetUTCSecondsImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("getMilliseconds", global.FunctionClass.New<JsDictionaryObject>(GetMillisecondsImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("getUTCMilliseconds", global.FunctionClass.New<JsDictionaryObject>(GetUTCMillisecondsImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("getTimezoneOffset", global.FunctionClass.New<JsDictionaryObject>(GetTimezoneOffsetImpl), PropertyAttributes.DontEnum);

            prototype.DefineOwnProperty("setTime", global.FunctionClass.New<JsDictionaryObject>(SetTimeImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("setMilliseconds", global.FunctionClass.New<JsDictionaryObject>(SetMillisecondsImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("setUTCMilliseconds", global.FunctionClass.New<JsDictionaryObject>(SetUTCMillisecondsImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("setSeconds", global.FunctionClass.New<JsDictionaryObject>(SetSecondsImpl, 2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("setUTCSeconds", global.FunctionClass.New<JsDictionaryObject>(SetUTCSecondsImpl, 2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("setMinutes", global.FunctionClass.New<JsDictionaryObject>(SetMinutesImpl, 3), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("setUTCMinutes", global.FunctionClass.New<JsDictionaryObject>(SetUTCMinutesImpl, 3), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("setHours", global.FunctionClass.New<JsDictionaryObject>(SetHoursImpl, 4), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("setUTCHours", global.FunctionClass.New<JsDictionaryObject>(SetUTCHoursImpl, 4), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("setDate", global.FunctionClass.New<JsDictionaryObject>(SetDateImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("setUTCDate", global.FunctionClass.New<JsDictionaryObject>(SetUTCDateImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("setMonth", global.FunctionClass.New<JsDictionaryObject>(SetMonthImpl, 2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("setUTCMonth", global.FunctionClass.New<JsDictionaryObject>(SetUTCMonthImpl, 2), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("setFullYear", global.FunctionClass.New<JsDictionaryObject>(SetFullYearImpl, 3), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("setUTCFullYear", global.FunctionClass.New<JsDictionaryObject>(SetUTCFullYearImpl, 3), PropertyAttributes.DontEnum);

            prototype.DefineOwnProperty("toUTCString", global.FunctionClass.New<JsDictionaryObject>(ToUTCStringImpl), PropertyAttributes.DontEnum);
            #endregion

            return prototype;
        }
        public JsDateConstructor(JsGlobal global)
            : this(global, true)
        {
        }

        public JsDate New()
        {
            return new JsDate(Prototype);
        }

        public JsDate New(double value)
        {
            return new JsDate(value, Prototype);
        }

        public JsDate New(DateTime value)
        {
            return new JsDate(value.ToUniversalTime(), Prototype);
        }

        public JsDate New(DateTime value, JsObject prototype)
        {
            return new JsDate(value, prototype);
        }

        public JsDate Construct(JsInstance[] parameters)
        {
            switch (parameters.Length)
            {
                case 0:
                    return New(DateTime.UtcNow);

                case 1:
                    if ((parameters[0].Class == JsInstance.ClassNumber || parameters[0].Class == JsInstance.ClassObject) && double.IsNaN(parameters[0].ToNumber()))
                        return New(double.NaN);
                    if (parameters[0].Class == JsInstance.ClassNumber)
                        return New(parameters[0].ToNumber());

                    double d;
                    if (ParseDate(Global, parameters[0].ToString(), CultureInfo.InvariantCulture, out d))
                        return New(d);
                    if (ParseDate(Global, parameters[0].ToString(), CultureInfo.CurrentCulture, out d))
                        return New(d);
                    return New(0);

                default:
                    var date = new DateTime(0, DateTimeKind.Local);

                    if (parameters.Length > 0)
                    {
                        int year = (int)parameters[0].ToNumber() - 1;
                        if (year < 100)
                            year += 1900;

                        date = date.AddYears(year);
                    }

                    if (parameters.Length > 1)
                        date = date.AddMonths((int)parameters[1].ToNumber());
                    if (parameters.Length > 2)
                        date = date.AddDays((int)parameters[2].ToNumber() - 1);
                    if (parameters.Length > 3)
                        date = date.AddHours((int)parameters[3].ToNumber());
                    if (parameters.Length > 4)
                        date = date.AddMinutes((int)parameters[4].ToNumber());
                    if (parameters.Length > 5)
                        date = date.AddSeconds((int)parameters[5].ToNumber());
                    if (parameters.Length > 6)
                        date = date.AddMilliseconds((int)parameters[6].ToNumber());

                    return New(date);
            }
        }

        public override JsFunctionResult Execute(JsGlobal global, JsInstance that, JsInstance[] parameters, Type[] genericArguments)
        {
            JsDate date = Construct(parameters);

            if (that == null || (that as JsGlobal) == global)
            {
                var result = ToStringImpl(date, Empty);
                return new JsFunctionResult(result, result);
            }

            return new JsFunctionResult(null, date);
        }

        private static bool ParseDate(JsGlobal global, string p, IFormatProvider culture, out double result)
        {
            result = 0;

            DateTime date;
            if (DateTime.TryParse(p, culture, DateTimeStyles.None, out date))
            {
                result = global.DateClass.New(date).ToNumber();
                return true;
            }

            if (DateTime.TryParseExact(p, JsDate.Format, culture, DateTimeStyles.None, out date))
            {
                result = global.DateClass.New(date).ToNumber();
                return true;
            }

            DateTime ld;

            if (DateTime.TryParseExact(p, JsDate.DateFormat, culture, DateTimeStyles.None, out ld))
            {
                date = date.AddTicks(ld.Ticks);
            }

            if (DateTime.TryParseExact(p, JsDate.TimeFormat, culture, DateTimeStyles.None, out ld))
            {
                date = date.AddTicks(ld.Ticks);
            }

            if (date.Ticks > 0)
            {
                result = global.DateClass.New(date).ToNumber();
                return true;
            }

            return true;
        }

        /// <summary>
        /// 15.9.4.1
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static JsInstance ParseImpl(JsGlobal global, JsInstance[] parameters)
        {
            double d;
            if (ParseDate(global, parameters[0].ToString(), CultureInfo.InvariantCulture, out d))
                return JsNumber.Create(d);
            else
                return JsNumber.NaN;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static JsInstance ParseLocaleImpl(JsGlobal global, JsInstance[] parameters)
        {
            double d;
            if (ParseDate(global, parameters[0].ToString(), CultureInfo.CurrentCulture, out d))
                return JsNumber.Create(d);
            return JsNumber.NaN;
        }

        internal static DateTime CreateDateTime(double number)
        {
            return new DateTime((long)(number * JsDate.TicksFactor + JsDate.Offset1970), DateTimeKind.Utc);
        }

        /// <summary>
        /// 15.9.5.2
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ToStringImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsString.Create(double.NaN.ToString());

            return JsString.Create(CreateDateTime(target.ToNumber()).ToLocalTime().ToString(JsDate.Format, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ToLocaleStringImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsString.Create(double.NaN.ToString());

            return JsString.Create(CreateDateTime(target.ToNumber()).ToLocalTime().ToString("F", CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// 15.9.5.3
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ToDateStringImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsString.Create(double.NaN.ToString());

            return JsString.Create(CreateDateTime(target.ToNumber()).ToLocalTime().ToString(JsDate.DateFormat, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 15.9.5.4
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ToTimeStringImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsString.Create(double.NaN.ToString());

            return JsString.Create(CreateDateTime(target.ToNumber()).ToLocalTime().ToString(JsDate.TimeFormat, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 15.9.5.6
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ToLocaleDateStringImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsString.Create(double.NaN.ToString());

            return JsString.Create(CreateDateTime(target.ToNumber()).ToLocalTime().ToString(JsDate.DateFormat));
        }

        /// <summary>
        /// 15.9.5.7
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ToLocaleTimeStringImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsString.Create(double.NaN.ToString());

            return JsString.Create(CreateDateTime(target.ToNumber()).ToLocalTime().ToString(JsDate.TimeFormat));
        }

        /// <summary>
        /// 15.9.5.8
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ValueOfImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsNumber.NaN;

            return JsNumber.Create(target.ToNumber());
        }

        /// <summary>
        /// 15.9.5.9
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance GetTimeImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsNumber.NaN;

            return JsNumber.Create(target.ToNumber());
        }

        /// <summary>
        /// 15.9.5.10
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance GetFullYearImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsNumber.NaN;

            return JsNumber.Create(CreateDateTime(target.ToNumber()).ToLocalTime().Year);
        }

        /// <summary>
        /// 15.9.5.11
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance GetUTCFullYearImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsNumber.NaN;

            return JsNumber.Create(CreateDateTime(target.ToNumber()).ToUniversalTime().Year);
        }

        /// <summary>
        /// 15.9.5.12
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance GetMonthImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
            {
                return JsNumber.NaN;
            }

            return JsNumber.Create(CreateDateTime(target.ToNumber()).ToLocalTime().Month - 1);
        }

        /// <summary>
        /// 15.9.5.13
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance GetUTCMonthImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsNumber.NaN;

            return JsNumber.Create(CreateDateTime(target.ToNumber()).ToUniversalTime().Month - 1);

        }

        /// <summary>
        /// 15.9.5.14
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance GetDateImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsNumber.NaN;

            return JsNumber.Create(CreateDateTime(target.ToNumber()).ToLocalTime().Day);
        }

        /// <summary>
        /// 15.9.5.15
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance GetUTCDateImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsNumber.NaN;

            return JsNumber.Create(CreateDateTime(target.ToNumber()).ToUniversalTime().Day);
        }

        /// <summary>
        /// 15.9.5.16
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance GetDayImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsNumber.NaN;

            return JsNumber.Create((int)CreateDateTime(target.ToNumber()).ToLocalTime().DayOfWeek);
        }

        /// <summary>
        /// 15.9.5.17
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance GetUTCDayImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsNumber.NaN;

            return JsNumber.Create((int)CreateDateTime(target.ToNumber()).ToUniversalTime().DayOfWeek);
        }

        /// <summary>
        /// 15.9.5.18
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance GetHoursImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsNumber.NaN;

            return JsNumber.Create(CreateDateTime(target.ToNumber()).ToLocalTime().Hour);
        }

        /// <summary>
        /// 15.9.5.19
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance GetUTCHoursImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsNumber.NaN;

            return JsNumber.Create(CreateDateTime(target.ToNumber()).ToUniversalTime().Hour);
        }

        /// <summary>
        /// 15.9.5.20
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance GetMinutesImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsNumber.NaN;

            return JsNumber.Create(CreateDateTime(target.ToNumber()).ToLocalTime().Minute);
        }

        /// <summary>
        /// 15.9.5.21
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance GetUTCMinutesImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
            {
                return JsNumber.NaN;
            }

            return JsNumber.Create(CreateDateTime(target.ToNumber()).ToUniversalTime().Minute);
        }

        /// <summary>
        /// 15.9.5.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ToUTCStringImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsString.Create(double.NaN.ToString());

            return JsString.Create(CreateDateTime(target.ToNumber()).ToString(JsDate.FormatUtc, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 15.9.5.22
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance GetSecondsImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsNumber.NaN;

            return JsNumber.Create(CreateDateTime(target.ToNumber()).ToLocalTime().Second);
        }

        /// <summary>
        /// 15.9.5.23
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance GetUTCSecondsImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsNumber.NaN;

            return JsNumber.Create(CreateDateTime(target.ToNumber()).ToUniversalTime().Second);
        }

        /// <summary>
        /// 15.9.5.24
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance GetMillisecondsImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsNumber.NaN;

            return JsNumber.Create(CreateDateTime(target.ToNumber()).ToLocalTime().Millisecond);
        }

        /// <summary>
        /// 15.9.5.25
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance GetUTCMillisecondsImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (double.IsNaN(target.ToNumber()))
                return JsNumber.NaN;

            return JsNumber.Create(CreateDateTime(target.ToNumber()).ToUniversalTime().Millisecond);
        }

        /// <summary>
        /// 15.9.5.26
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance GetTimezoneOffsetImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            return JsNumber.Create(-TimeZone.CurrentTimeZone.GetUtcOffset(new DateTime()).TotalMinutes);
        }

        /// <summary>
        /// 15.9.5.27
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SetTimeImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
                throw new ArgumentException("There was no tiem specified");

            target.Value = parameters[0].ToNumber();
            return target;
        }

        /// <summary>
        /// 15.9.5.28
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SetMillisecondsImpl(JsGlobal global, JsDictionaryObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
                throw new ArgumentException("There was no millisecond specified");

            DateTime valueOf = CreateDateTime(target.ToNumber()).ToLocalTime();
            valueOf = valueOf.AddMilliseconds(-valueOf.Millisecond);
            valueOf = valueOf.AddMilliseconds(parameters[0].ToNumber());
            target.Value = global.DateClass.New(valueOf).ToNumber();
            return target;
        }

        /// <summary>
        /// 15.9.5.29
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SetUTCMillisecondsImpl(JsGlobal global, JsDictionaryObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
                throw new ArgumentException("There was no millisecond specified");

            DateTime valueOf = CreateDateTime(target.ToNumber());
            valueOf = valueOf.AddMilliseconds(-valueOf.Millisecond);
            valueOf = valueOf.AddMilliseconds(parameters[0].ToNumber());
            target.Value = global.DateClass.New(valueOf).ToNumber();
            return target;
        }

        /// <summary>
        /// 15.9.5.30
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SetSecondsImpl(JsGlobal global, JsDictionaryObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
                throw new ArgumentException("There was no second specified");
            DateTime valueOf = CreateDateTime(target.ToNumber()).ToLocalTime();
            valueOf = valueOf.AddSeconds(-valueOf.Second);
            valueOf = valueOf.AddSeconds(parameters[0].ToNumber());
            target.Value = valueOf;
            if (parameters.Length > 1)
            {
                JsInstance[] innerParams = new JsInstance[parameters.Length - 1];
                Array.Copy(parameters, 1, innerParams, 0, innerParams.Length);
                target = (JsDate)SetMillisecondsImpl(global, target, innerParams);
            }
            return target;
        }

        /// <summary>
        /// 15.9.5.31
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SetUTCSecondsImpl(JsGlobal global, JsDictionaryObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
                throw new ArgumentException("There was no second specified");
            DateTime valueOf = CreateDateTime(target.ToNumber());
            valueOf = valueOf.AddSeconds(-valueOf.Second);
            valueOf = valueOf.AddSeconds(parameters[0].ToNumber());
            target.Value = valueOf;
            if (parameters.Length > 1)
            {
                JsInstance[] innerParams = new JsInstance[parameters.Length - 1];
                Array.Copy(parameters, 1, innerParams, 0, innerParams.Length);
                target = (JsDate)SetMillisecondsImpl(global, target, innerParams);
            }
            return target;
        }

        /// <summary>
        /// 15.9.5.33
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SetMinutesImpl(JsGlobal global, JsDictionaryObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
                throw new ArgumentException("There was no minute specified");
            DateTime valueOf = CreateDateTime(target.ToNumber()).ToLocalTime();
            valueOf = valueOf.AddMinutes(-valueOf.Minute);
            valueOf = valueOf.AddMinutes(parameters[0].ToNumber());
            target.Value = valueOf;
            if (parameters.Length > 1)
            {
                JsInstance[] innerParams = new JsInstance[parameters.Length - 1];
                Array.Copy(parameters, 1, innerParams, 0, innerParams.Length);
                target = (JsDate)SetSecondsImpl(global, target, innerParams);
            }
            return target;
        }

        /// <summary>
        /// 15.9.5.34
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SetUTCMinutesImpl(JsGlobal global, JsDictionaryObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
                throw new ArgumentException("There was no minute specified");
            DateTime valueOf = CreateDateTime(target.ToNumber());
            valueOf = valueOf.AddMinutes(-valueOf.Minute);
            valueOf = valueOf.AddMinutes(parameters[0].ToNumber());
            target.Value = valueOf;
            if (parameters.Length > 1)
            {
                JsInstance[] innerParams = new JsInstance[parameters.Length - 1];
                Array.Copy(parameters, 1, innerParams, 0, innerParams.Length);
                target = (JsDate)SetSecondsImpl(global, target, innerParams);
            }
            return target;
        }

        /// <summary>
        /// 15.9.5.35
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SetHoursImpl(JsGlobal global, JsDictionaryObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
                throw new ArgumentException("There was no hour specified");
            DateTime valueOf = CreateDateTime(target.ToNumber()).ToLocalTime();
            valueOf = valueOf.AddHours(-valueOf.Hour);
            valueOf = valueOf.AddHours(parameters[0].ToNumber());
            target.Value = valueOf;
            if (parameters.Length > 1)
            {
                JsInstance[] innerParams = new JsInstance[parameters.Length - 1];
                Array.Copy(parameters, 1, innerParams, 0, innerParams.Length);
                target = (JsDate)SetMinutesImpl(global, target, innerParams);
            }
            return target;
        }

        /// <summary>
        /// 15.9.5.36
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SetUTCHoursImpl(JsGlobal global, JsDictionaryObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
                throw new ArgumentException("There was no hour specified");
            DateTime valueOf = CreateDateTime(target.ToNumber());
            valueOf = valueOf.AddHours(-valueOf.Hour);
            valueOf = valueOf.AddHours(parameters[0].ToNumber());
            target.Value = valueOf;
            if (parameters.Length > 1)
            {
                JsInstance[] innerParams = new JsInstance[parameters.Length - 1];
                Array.Copy(parameters, 1, innerParams, 0, innerParams.Length);
                target = (JsDate)SetMinutesImpl(global, target, innerParams);
            }
            return target;
        }

        /// <summary>
        /// 15.9.5.36
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SetDateImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
                throw new ArgumentException("There was no date specified");
            DateTime valueOf = CreateDateTime(target.ToNumber()).ToLocalTime();
            valueOf = valueOf.AddDays(-valueOf.Day);
            valueOf = valueOf.AddDays(parameters[0].ToNumber());
            target.Value = valueOf;
            return target;

        }

        /// <summary>
        /// 15.9.5.37
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SetUTCDateImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
                throw new ArgumentException("There was no date specified");
            DateTime valueOf = CreateDateTime(target.ToNumber());
            valueOf = valueOf.AddDays(-valueOf.Day);
            valueOf = valueOf.AddDays(parameters[0].ToNumber());
            target.Value = valueOf;
            return target;
        }

        /// <summary>
        /// 15.9.5.38
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SetMonthImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
                throw new ArgumentException("There was no month specified");
            DateTime valueOf = CreateDateTime(target.ToNumber()).ToLocalTime();
            valueOf = valueOf.AddMonths(-valueOf.Month);
            valueOf = valueOf.AddMonths((int)parameters[0].ToNumber());
            target.Value = valueOf;
            if (parameters.Length > 1)
            {
                JsInstance[] innerParams = new JsInstance[parameters.Length - 1];
                Array.Copy(parameters, 1, innerParams, 0, innerParams.Length);
                target = (JsDate)SetDateImpl(target, innerParams);
            }
            return target;
        }

        /// <summary>
        /// 15.9.5.39
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SetUTCMonthImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
                throw new ArgumentException("There was no month specified");
            DateTime valueOf = CreateDateTime(target.ToNumber());
            valueOf = valueOf.AddMonths(-valueOf.Month);
            valueOf = valueOf.AddMonths((int)parameters[0].ToNumber());
            target.Value = valueOf;
            if (parameters.Length > 1)
            {
                JsInstance[] innerParams = new JsInstance[parameters.Length - 1];
                Array.Copy(parameters, 1, innerParams, 0, innerParams.Length);
                target = (JsDate)SetDateImpl(target, innerParams);
            }
            return target;
        }

        /// <summary>
        /// 15.9.5.40
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SetFullYearImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
                throw new ArgumentException("There was no year specified");
            DateTime valueOf = CreateDateTime(target.ToNumber()).ToLocalTime();
            int diff = valueOf.Year - (int)parameters[0].ToNumber();
            valueOf = valueOf.AddYears(-diff);
            target.Value = valueOf;
            if (parameters.Length > 1)
            {
                JsInstance[] innerParams = new JsInstance[parameters.Length - 1];
                Array.Copy(parameters, 1, innerParams, 0, innerParams.Length);
                target = (JsDate)SetMonthImpl(target, innerParams);
            }
            return target;

        }

        /// <summary>
        /// 15.9.5.41
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance SetUTCFullYearImpl(JsDictionaryObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
                throw new ArgumentException("There was no year specified");
            DateTime valueOf = CreateDateTime(target.ToNumber());
            valueOf = valueOf.AddYears(-valueOf.Year);
            valueOf = valueOf.AddYears((int)parameters[0].ToNumber());
            target.Value = valueOf;
            if (parameters.Length > 1)
            {
                JsInstance[] innerParams = new JsInstance[parameters.Length - 1];
                Array.Copy(parameters, 1, innerParams, 0, innerParams.Length);
                target = (JsDate)SetMonthImpl(target, innerParams);
            }
            return target;
        }

        public static JsInstance UTCImpl(JsGlobal global, JsInstance[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] is JsUndefined // undefined
                    || (parameters[i].Class == JsInstance.ClassNumber && double.IsNaN(parameters[i].ToNumber())) // NaN
                    || (parameters[i].Class == JsInstance.ClassNumber && double.IsInfinity(parameters[i].ToNumber())) // Infinity
                    //|| parameters[i].Class == JsInstance.CLASS_OBJECT // don't accept objects ???!
                    )
                {
                    return JsNumber.NaN;
                }
            }

            JsDate result = global.DateClass.Construct(parameters);
            double offset = result.ToNumber() + TimeZone.CurrentTimeZone.GetUtcOffset(new DateTime()).TotalMilliseconds;
            return JsNumber.Create(offset);
        }
    }
}
