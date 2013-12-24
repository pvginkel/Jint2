using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static class NumberFunctions
        {
            public static object Constructor(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var target = (JsObject)@this;
                if (target == runtime.Global.GlobalScope)
                {
                    // 15.7.1 - When Number is called as a function rather than as a constructor, it performs a type conversion.
                    if (arguments.Length > 0)
                        return JsValue.ToNumber(arguments[0]);

                    return (double)0;
                }

                // 15.7.2 - When Number is called as part of a new expression, it is a constructor: it initializes the newly created object.
                target.Value = arguments.Length > 0 ? JsValue.ToNumber(arguments[0]) : 0;

                return @this;
            }

            public static object ValueOf(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                if (@this is double)
                    return @this;

                return Convert.ToDouble(JsValue.UnwrapValue(@this));
            }

            public static object ToLocaleString(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                // Remove arguments
                return ((JsObject)((JsObject)@this).GetProperty(Id.toString)).Execute(runtime, @this, JsValue.EmptyArray);
            }

            private static readonly char[] RDigits =
            {
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 
                'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 
                'U', 'V', 'W', 'X', 'Y', 'Z'
            };

            public static object ToString(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                double value = JsValue.ToNumber(@this);

                if (Double.IsNaN(value))
                    return "NaN";
                if (Double.IsNegativeInfinity(value))
                    return "-Infinity";
                if (Double.IsPositiveInfinity(value))
                    return "Infinity";

                int radix = 10;

                // Is radix defined ?
                if (
                    arguments.Length > 0 &&
                    !JsValue.IsUndefined(arguments[0])
                )
                    radix = (int)JsValue.ToNumber(arguments[0]);

                var longToBeFormatted = (long)value;

                if (radix == 10)
                    return value.ToString(CultureInfo.InvariantCulture).ToLower();

                // Extract the magnitude for conversion.
                long longPositive = Math.Abs(longToBeFormatted);
                int digitIndex;

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

                return new string(outDigits,
                    outDigits.Length - digitIndex, digitIndex).ToLower();
            }

            // 15.7.4.5
            public static object ToFixed(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                int fractions = 0;
                if (arguments.Length > 0)
                    fractions = (int)JsValue.ToNumber(arguments[0]);

                if (fractions > 20 || fractions < 0)
                    throw new JsException(JsErrorType.SyntaxError, "Fraction Digits must be greater than 0 and lesser than 20");

                double value = JsValue.ToNumber(@this);

                if (Double.IsNaN(value))
                    return JsValue.ToString(@this);

                return value.ToString("f" + fractions, CultureInfo.InvariantCulture);
            }

            // 15.7.4.6
            public static object ToExponential(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                double value = JsValue.ToNumber(@this);

                if (Double.IsInfinity(value) || Double.IsNaN(value))
                    return ((JsObject)((JsObject)@this).GetProperty(Id.toString)).Execute(runtime, @this, JsValue.EmptyArray);

                int fractions = 16;
                if (arguments.Length > 0)
                    fractions = (int)JsValue.ToNumber(arguments[0]);

                if (fractions > 20 || fractions < 0)
                    throw new JsException(JsErrorType.SyntaxError, "Fraction Digits must be greater than 0 and lesser than 20");

                string format = String.Concat("#.", new String('0', fractions), "e+0");
                return value.ToString(format, CultureInfo.InvariantCulture);
            }

            // 15.7.4.7
            public static object ToPrecision(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                double value = JsValue.ToNumber(@this);

                if (Double.IsInfinity(value) || Double.IsNaN(value))
                    return ((JsObject)((JsObject)@this).GetProperty(Id.toString)).Execute(runtime, @this, JsValue.EmptyArray);

                if (arguments.Length == 0)
                    throw new JsException(JsErrorType.SyntaxError, "Precision missing");

                if (JsValue.IsUndefined(arguments[0]))
                    return ((JsObject)((JsObject)@this).GetProperty(Id.toString)).Execute(runtime, @this, JsValue.EmptyArray);

                int precision = 0;
                if (arguments.Length > 0)
                {
                    precision = (int)JsValue.ToNumber(arguments[0]);
                }

                if (precision < 1 || precision > 21)
                {
                    throw new JsException(JsErrorType.RangeError, "Precision must be between 1 and 21");
                }

                // Get the number of decimals
                string stringValue = value.ToString("e23", CultureInfo.InvariantCulture);
                int decimals = stringValue.IndexOfAny(new[] { '.', 'e' });
                decimals = decimals == -1 ? stringValue.Length : decimals;

                precision -= decimals;
                precision = precision < 1 ? 1 : precision;

                return value.ToString("f" + precision, CultureInfo.InvariantCulture);
            }
        }
    }
}
