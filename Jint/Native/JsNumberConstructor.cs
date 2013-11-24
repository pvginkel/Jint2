using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Jint.Expressions;

namespace Jint.Native
{
    [Serializable]
    public class JsNumberConstructor : JsConstructor
    {
        public JsNumberConstructor(JsGlobal global)
            : base(global, BuildPrototype(global))
        {
            Name = "Number";

            DefineOwnProperty("MAX_VALUE", JsNumber.MaxValue);
            DefineOwnProperty("MIN_VALUE", JsNumber.MinValue);
            DefineOwnProperty("NaN", JsNumber.NaN);
            DefineOwnProperty("POSITIVE_INFINITY", JsNumber.PositiveInfinity);
            DefineOwnProperty("NEGATIVE_INFINITY", JsNumber.NegativeInfinity);
        }

        private static JsObject BuildPrototype(JsGlobal global)
        {
            var prototype = new JsObject(global, global.FunctionClass.Prototype);

            prototype.DefineOwnProperty("toString", global.FunctionClass.New<JsInstance>(ToStringImpl, 1), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toLocaleString", global.FunctionClass.New<JsInstance>(ToLocaleStringImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toFixed", global.FunctionClass.New<JsInstance>(ToFixedImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toExponential", global.FunctionClass.New<JsInstance>(ToExponentialImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("toPrecision", global.FunctionClass.New<JsInstance>(ToPrecisionImpl), PropertyAttributes.DontEnum);
            prototype.DefineOwnProperty("valueOf", global.FunctionClass.New<JsObject>(ValueOfImpl), PropertyAttributes.DontEnum);

            return prototype;
        }

        public static JsInstance ValueOfImpl(JsObject target, JsInstance[] parameters)
        {
            return JsNumber.Create(Convert.ToDouble(target.Value));
        }

        public override JsFunctionResult Execute(JsInstance that, JsInstance[] parameters, Type[] genericArguments)
        {
            if (that == null || that == Global.GlobalScope)
            {
                JsInstance result;

                // 15.7.1 - When Number is called as a function rather than as a constructor, it performs a type conversion.
                if (parameters.Length > 0)
                    result = JsNumber.Create(parameters[0].ToNumber());
                else
                    result = JsNumber.Create(0);

                return new JsFunctionResult(result, result);
            }
            else
            {
                // 15.7.2 - When Number is called as part of a new expression, it is a constructor: it initialises the newly created object.
                if (parameters.Length > 0)
                    that.Value = parameters[0].ToNumber();
                else
                    that.Value = 0;

                return new JsFunctionResult(that, that);
            }
        }

        public static JsInstance ToLocaleStringImpl(JsInstance target, JsInstance[] parameters)
        {
            // Remove parameters
            return ToStringImpl(target, new JsInstance[0]);
        }

        private static readonly char[] RDigits =
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 
            'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 
            'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        public static JsInstance ToStringImpl(JsInstance target, JsInstance[] parameters)
        {
            if (target == JsNumber.NaN)
                return JsString.Create("NaN");

            if (target == JsNumber.NegativeInfinity)
                return JsString.Create("-Infinity");

            if (target == JsNumber.PositiveInfinity)
                return JsString.Create("Infinity");

            int radix = 10;

            // is radix defined ?
            if (parameters.Length > 0)
            {
                if (!IsUndefined(parameters[0]))
                {
                    radix = (int)parameters[0].ToNumber();
                }
            }

            var longToBeFormatted = (long)target.ToNumber();

            if (radix == 10)
            {
                return JsString.Create(target.ToNumber().ToString(CultureInfo.InvariantCulture).ToLower());
            }
            else
            {
                // Extract the magnitude for conversion.
                long longPositive = Math.Abs(longToBeFormatted);
                int digitIndex = 0;

                char[] outDigits = new char[63];
                // Convert the magnitude to a digit string.
                for (digitIndex = 0; digitIndex <= 64; digitIndex++)
                {
                    if (longPositive == 0) break;

                    outDigits[outDigits.Length - digitIndex - 1] =
                        RDigits[longPositive % radix];
                    longPositive /= radix;
                }

                // Add a minus sign if the argument is negative.
                if (longToBeFormatted < 0)
                    outDigits[outDigits.Length - digitIndex++ - 1] = '-';

                return JsString.Create(new string(outDigits,
                    outDigits.Length - digitIndex, digitIndex).ToLower());
            }
        }

        /// <summary>
        /// 15.7.4.5
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ToFixedImpl(JsGlobal global, JsInstance target, JsInstance[] parameters)
        {
            int fractions = 0;
            if (parameters.Length > 0)
                fractions = Convert.ToInt32(parameters[0].ToNumber());

            if (fractions > 20 || fractions < 0)
                throw new JsException(global.SyntaxErrorClass.New("Fraction Digits must be greater than 0 and lesser than 20"));

            if (target == JsNumber.NaN)
                return JsString.Create(target.ToString());

            return JsString.Create(target.ToNumber().ToString("f" + fractions, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 15.7.4.6
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ToExponentialImpl(JsGlobal global, JsInstance target, JsInstance[] parameters)
        {
            if (double.IsInfinity(target.ToNumber()) || double.IsNaN(target.ToNumber()))
                return ToStringImpl(target, new JsInstance[0]);

            int fractions = 16;
            if (parameters.Length > 0)
            {
                fractions = Convert.ToInt32(parameters[0].ToNumber());
            }

            if (fractions > 20 || fractions < 0)
            {
                throw new JsException(global.SyntaxErrorClass.New("Fraction Digits must be greater than 0 and lesser than 20"));
            }

            string format = String.Concat("#.", new String('0', fractions), "e+0");
            return JsString.Create(target.ToNumber().ToString(format, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 15.7.4.7
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static JsInstance ToPrecisionImpl(JsGlobal global, JsInstance target, JsInstance[] parameters)
        {
            if (double.IsInfinity(target.ToNumber()) || double.IsNaN(target.ToNumber()))
                return ToStringImpl(target, new JsInstance[0]);

            if (parameters.Length == 0)
                throw new JsException(global.SyntaxErrorClass.New("precision missing"));

            if (IsUndefined(parameters[0]))
                return ToStringImpl(target, new JsInstance[0]);

            int precision = 0;
            if (parameters.Length > 0)
            {
                precision = Convert.ToInt32(parameters[0].ToNumber());
            }

            if (precision < 1 || precision > 21)
            {
                throw new JsException(global.RangeErrorClass.New("precision must be between 1 and 21"));
            }

            // Get the number of decimals
            string str = target.ToNumber().ToString("e23", CultureInfo.InvariantCulture);
            int decimals = str.IndexOfAny(new char[] { '.', 'e' });
            decimals = decimals == -1 ? str.Length : decimals;

            precision -= decimals;
            precision = precision < 1 ? 1 : precision;

            return JsString.Create(target.ToNumber().ToString("f" + precision, CultureInfo.InvariantCulture));
        }
    }
}
