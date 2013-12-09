using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static readonly char[] ReservedEncoded = new[] { ';', ',', '/', '?', ':', '@', '&', '=', '+', '$', '#' };
        private static readonly char[] ReservedEncodedComponent = new[] { '-', '_', '.', '!', '~', '*', '\'', '(', ')', '[', ']' };

        private JsObject CreateGlobalScope()
        {
            var scope = CreateObject();

            scope.SetClass(JsNames.ClassGlobal);
            scope.IsClr = false;
            scope.PropertyStore = new DictionaryPropertyStore(scope);

            scope.DefineProperty("null", JsNull.Instance, PropertyAttributes.DontEnum);
            scope.DefineProperty("Function", FunctionClass, PropertyAttributes.DontEnum);
            scope.DefineProperty("Object", ObjectClass, PropertyAttributes.DontEnum);
            scope.DefineProperty("Array", ArrayClass, PropertyAttributes.DontEnum);
            scope.DefineProperty("Boolean", BooleanClass, PropertyAttributes.DontEnum);
            scope.DefineProperty("Date", DateClass, PropertyAttributes.DontEnum);
            scope.DefineProperty("Error", ErrorClass, PropertyAttributes.DontEnum);
            scope.DefineProperty("EvalError", EvalErrorClass, PropertyAttributes.DontEnum);
            scope.DefineProperty("RangeError", RangeErrorClass, PropertyAttributes.DontEnum);
            scope.DefineProperty("ReferenceError", ReferenceErrorClass, PropertyAttributes.DontEnum);
            scope.DefineProperty("SyntaxError", SyntaxErrorClass, PropertyAttributes.DontEnum);
            scope.DefineProperty("TypeError", TypeErrorClass, PropertyAttributes.DontEnum);
            scope.DefineProperty("URIError", URIErrorClass, PropertyAttributes.DontEnum);
            scope.DefineProperty("Number", NumberClass, PropertyAttributes.DontEnum);
            scope.DefineProperty("RegExp", RegExpClass, PropertyAttributes.DontEnum);
            scope.DefineProperty("String", StringClass, PropertyAttributes.DontEnum);
            scope.DefineProperty("Math", MathClass, PropertyAttributes.DontEnum);
            scope.DefineProperty("NaN", NumberClass.GetProperty(Id.NaN), PropertyAttributes.DontEnum); // 15.1.1.1
            scope.DefineProperty("Infinity", NumberClass.GetProperty(Id.POSITIVE_INFINITY), PropertyAttributes.DontEnum); // 15.1.1.2
            scope.DefineProperty("undefined", JsUndefined.Instance, PropertyAttributes.DontEnum); // 15.1.1.3
            scope.DefineProperty(JsNames.This, scope, PropertyAttributes.DontEnum);
            scope.DefineProperty("ToBoolean", GlobalFunctions.ToBoolean, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("ToByte", GlobalFunctions.ToByte, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("ToChar", GlobalFunctions.ToChar, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("ToDateTime", GlobalFunctions.ToDateTime, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("ToDecimal", GlobalFunctions.ToDecimal, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("ToDouble", GlobalFunctions.ToDouble, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("ToInt16", GlobalFunctions.ToInt16, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("ToInt32", GlobalFunctions.ToInt32, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("ToInt64", GlobalFunctions.ToInt64, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("ToSByte", GlobalFunctions.ToSByte, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("ToSingle", GlobalFunctions.ToSingle, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("ToString", GlobalFunctions.ToString, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("ToUInt16", GlobalFunctions.ToUInt16, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("ToUInt32", GlobalFunctions.ToUInt32, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("ToUInt64", GlobalFunctions.ToUInt64, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("eval", GlobalFunctions.Eval, 1, PropertyAttributes.DontEnum); // 15.1.2.1
            scope.DefineProperty("parseInt", GlobalFunctions.ParseInt, 1, PropertyAttributes.DontEnum); // 15.1.2.2
            scope.DefineProperty("parseFloat", GlobalFunctions.ParseFloat, 1, PropertyAttributes.DontEnum); // 15.1.2.3
            scope.DefineProperty("isNaN", GlobalFunctions.IsNaN, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("isFinite", GlobalFunctions.IsFinite, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("decodeURI", GlobalFunctions.DecodeURI, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("encodeURI", GlobalFunctions.EncodeURI, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("decodeURIComponent", GlobalFunctions.DecodeURIComponent, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty("encodeURIComponent", GlobalFunctions.EncodeURIComponent, 1, PropertyAttributes.DontEnum);

            return scope;
        }

        private static class GlobalFunctions
        {
            public static object ToBoolean(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return BooleanBoxes.Box(Convert.ToBoolean(JsValue.UnwrapValue(arguments[0])));
            }

            public static object ToByte(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)Convert.ToByte(JsValue.UnwrapValue(arguments[0]));
            }

            public static object ToChar(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)Convert.ToChar(JsValue.UnwrapValue(arguments[0]));
            }

            public static object ToDateTime(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return runtime.Global.CreateDate((DateTime)JsValue.UnwrapValue(arguments[0]));
            }

            public static object ToDecimal(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)Convert.ToDecimal(JsValue.UnwrapValue(arguments[0]));
            }

            public static object ToDouble(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return Convert.ToDouble(JsValue.UnwrapValue(arguments[0]));
            }

            public static object ToInt16(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)Convert.ToInt16(JsValue.UnwrapValue(arguments[0]));
            }

            public static object ToInt32(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)Convert.ToInt32(JsValue.UnwrapValue(arguments[0]));
            }

            public static object ToInt64(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)Convert.ToInt64(JsValue.UnwrapValue(arguments[0]));
            }

            public static object ToSByte(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)Convert.ToSByte(JsValue.UnwrapValue(arguments[0]));
            }

            public static object ToSingle(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)Convert.ToSingle(JsValue.UnwrapValue(arguments[0]));
            }

            public static object ToString(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return JsValue.ToString(@this);
            }

            public static object ToUInt16(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)Convert.ToUInt16(JsValue.UnwrapValue(arguments[0]));
            }

            public static object ToUInt32(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)Convert.ToUInt32(JsValue.UnwrapValue(arguments[0]));
            }

            public static object ToUInt64(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                return (double)Convert.ToUInt64(JsValue.UnwrapValue(arguments[0]));
            }

            /// <summary>
            /// 15.1.2.1
            /// </summary>
            public static object Eval(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (JsNames.ClassString != JsValue.GetClass(arguments[0]))
                    return arguments[0];

                return runtime.Global.Engine.Eval(arguments);
            }

            /// <summary>
            /// 15.1.2.2
            /// </summary>
            public static object ParseInt(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length < 1 || JsValue.IsUndefined(arguments[0]))
                    return JsUndefined.Instance;

                // In case of an enum, just cast it to an integer
                if (JsValue.IsClr(arguments[0]))
                {
                    var value = JsValue.UnwrapValue(arguments[0]);
                    if (value.GetType().IsEnum)
                        return (double)((int)value);
                }

                string number = JsValue.ToString(arguments[0]).Trim();
                int sign = 1;
                int radix = 10;

                if (number == String.Empty)
                    return DoubleBoxes.NaN;

                if (number.StartsWith("-"))
                {
                    number = number.Substring(1);
                    sign = -1;
                }
                else if (number.StartsWith("+"))
                {
                    number = number.Substring(1);
                }

                if (
                    arguments.Length >= 2 &&
                    !JsValue.IsUndefined(arguments[1]) &&
                    JsValue.ToNumber(arguments[1]) != 0
                )
                    radix = (int)JsValue.ToNumber(arguments[1]);

                if (radix == 0)
                    radix = 10;
                else if (radix < 2 || radix > 36)
                    return DoubleBoxes.NaN;

                if (number.ToLower().StartsWith("0x"))
                    radix = 16;

                try
                {
                    if (radix == 10)
                    {
                        // most common case
                        double result;
                        if (double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                        {
                            // parseInt(12.42) == 42
                            return sign * Math.Floor(result);
                        }
                        else
                        {
                            return DoubleBoxes.NaN;
                        }
                    }
                    else
                    {
                        return (double)(sign * Convert.ToInt32(number, radix));
                    }
                }
                catch
                {
                    return DoubleBoxes.NaN;
                }
            }

            /// <summary>
            /// 15.1.2.3
            /// </summary>
            public static object ParseFloat(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length < 1 || JsValue.IsUndefined(arguments[0]))
                    return JsUndefined.Instance;

                string number = JsValue.ToString(arguments[0]).Trim();
                // the parseFloat function should stop parsing when it encounters an disallowed char
                Regex regexp = new Regex(@"^[\+\-\d\.e]*", RegexOptions.IgnoreCase);

                Match match = regexp.Match(number);

                double result;
                if (match.Success && double.TryParse(match.Value, NumberStyles.Float, new CultureInfo("en-US"), out result))
                    return result;
                else
                    return DoubleBoxes.NaN;
            }

            /// <summary>
            /// 15.1.2.4
            /// </summary>
            public static object IsNaN(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length < 1)
                {
                    return BooleanBoxes.Box(false);
                }

                return BooleanBoxes.Box(double.NaN.Equals(JsValue.ToNumber(arguments[0])));
            }

            /// <summary>
            /// 15.1.2.5
            /// </summary>
            public static object IsFinite(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length < 1 || JsValue.IsUndefined(arguments[0]))
                    return BooleanBoxes.False;

                var value = JsValue.ToNumber(arguments[0]);

                return BooleanBoxes.Box(
                    !Double.IsNaN(value) &&
                    !Double.IsPositiveInfinity(value) &&
                    !Double.IsNegativeInfinity(value)
                );
            }

            public static object DecodeURI(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length < 1 || JsValue.IsUndefined(arguments[0]))
                    return String.Empty;

                return Uri.UnescapeDataString(JsValue.ToString(arguments[0]).Replace("+", " "));
            }

            public static object EncodeURI(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length < 1 || JsValue.IsUndefined(arguments[0]))
                    return String.Empty;

                string encoded = Uri.EscapeDataString(JsValue.ToString(arguments[0]));

                foreach (char c in ReservedEncoded)
                {
                    encoded = encoded.Replace(Uri.EscapeDataString(c.ToString()), c.ToString());
                }

                foreach (char c in ReservedEncodedComponent)
                {
                    encoded = encoded.Replace(Uri.EscapeDataString(c.ToString()), c.ToString());
                }

                return encoded.ToUpper();
            }

            public static object DecodeURIComponent(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length < 1 || JsValue.IsUndefined(arguments[0]))
                    return String.Empty;

                return Uri.UnescapeDataString(JsValue.ToString(arguments[0]).Replace("+", " "));
            }

            public static object EncodeURIComponent(JintRuntime runtime, object @this, JsObject callee, object closure, object[] arguments, object[] genericArguments)
            {
                if (arguments.Length < 1 || JsValue.IsUndefined(arguments[0]))
                    return String.Empty;

                string encoded = Uri.EscapeDataString(JsValue.ToString(arguments[0]));

                foreach (char c in ReservedEncodedComponent)
                {
                    encoded = encoded.Replace(Uri.EscapeDataString(c.ToString()), c.ToString().ToUpper());
                }

                return encoded;
            }
        }
    }
}
