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
            public static JsInstance Constructor(Runtime.JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (@this == null || @this == runtime.Global.GlobalScope)
                {
                    // 15.7.1 - When Number is called as a function rather than as a constructor, it performs a type conversion.
                    if (arguments.Length > 0)
                        return JsNumber.Create(arguments[0].ToNumber());

                    return JsNumber.Create(0);
                }

                // 15.7.2 - When Number is called as part of a new expression, it is a constructor: it initializes the newly created object.
                @this.Value = arguments.Length > 0 ? arguments[0].ToNumber() : 0;

                return @this;
            }

            public static JsInstance ValueOf(Runtime.JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                var jsNumber = @this as JsNumber;
                if (jsNumber != null)
                    return @this;

                return JsNumber.Create(Convert.ToDouble(@this.Value));
            }

            public static JsInstance ToLocaleString(Runtime.JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                // Remove arguments
                return ((JsObject)((JsObject)@this).GetProperty(Id.toString)).Execute(runtime, @this, JsInstance.EmptyArray, null);
            }

            private static readonly char[] RDigits =
            {
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 
                'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 
                'U', 'V', 'W', 'X', 'Y', 'Z'
            };

            public static JsInstance ToString(Runtime.JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (@this == JsNumber.NaN)
                    return JsString.Create("NaN");

                if (@this == JsNumber.NegativeInfinity)
                    return JsString.Create("-Infinity");

                if (@this == JsNumber.PositiveInfinity)
                    return JsString.Create("Infinity");

                int radix = 10;
                double value = Convert.ToDouble(@this.Value);

                // is radix defined ?
                if (
                    arguments.Length > 0 &&
                    !JsInstance.IsUndefined(arguments[0])
                )
                    radix = (int)arguments[0].ToNumber();

                var longToBeFormatted = (long)value;

                if (radix == 10)
                    return JsString.Create(value.ToString(CultureInfo.InvariantCulture).ToLower());

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

                return JsString.Create(new string(outDigits,
                    outDigits.Length - digitIndex, digitIndex).ToLower());
            }

            // 15.7.4.5
            public static JsInstance ToFixed(Runtime.JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                int fractions = 0;
                if (arguments.Length > 0)
                    fractions = Convert.ToInt32(arguments[0].ToNumber());

                if (fractions > 20 || fractions < 0)
                    throw new JsException(JsErrorType.SyntaxError, "Fraction Digits must be greater than 0 and lesser than 20");

                if (@this == JsNumber.NaN)
                    return JsString.Create(@this.ToString());

                return JsString.Create(Convert.ToDouble(@this.Value).ToString("f" + fractions, CultureInfo.InvariantCulture));
            }

            // 15.7.4.6
            public static JsInstance ToExponential(Runtime.JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                double value = Convert.ToDouble(@this.Value);

                if (Double.IsInfinity(value) || Double.IsNaN(value))
                    return ((JsObject)((JsObject)@this).GetProperty(Id.toString)).Execute(runtime, @this, JsInstance.EmptyArray, null);

                int fractions = 16;
                if (arguments.Length > 0)
                {
                    fractions = Convert.ToInt32(arguments[0].ToNumber());
                }

                if (fractions > 20 || fractions < 0)
                {
                    throw new JsException(JsErrorType.SyntaxError, "Fraction Digits must be greater than 0 and lesser than 20");
                }

                string format = String.Concat("#.", new String('0', fractions), "e+0");
                return JsString.Create(value.ToString(format, CultureInfo.InvariantCulture));
            }

            // 15.7.4.7
            public static JsInstance ToPrecision(Runtime.JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                double value = Convert.ToDouble(@this.Value);

                if (Double.IsInfinity(value) || Double.IsNaN(value))
                    return ((JsObject)((JsObject)@this).GetProperty(Id.toString)).Execute(runtime, @this, JsInstance.EmptyArray, null);

                if (arguments.Length == 0)
                    throw new JsException(JsErrorType.SyntaxError, "Precision missing");

                if (JsInstance.IsUndefined(arguments[0]))
                    return ((JsObject)((JsObject)@this).GetProperty(Id.toString)).Execute(runtime, @this, JsInstance.EmptyArray, null);

                int precision = 0;
                if (arguments.Length > 0)
                {
                    precision = Convert.ToInt32(arguments[0].ToNumber());
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

                return JsString.Create(value.ToString("f" + precision, CultureInfo.InvariantCulture));
            }
        }
    }
}
