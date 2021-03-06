﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static readonly char[] ReservedEncoded = { ';', ',', '/', '?', ':', '@', '&', '=', '+', '$', '#' };
        private static readonly char[] ReservedEncodedComponent = { '-', '_', '.', '!', '~', '*', '\'', '(', ')', '[', ']' };

        private JsObject CreateGlobalScope()
        {
            var scope = CreateObject();

            scope.SetClass(JsNames.ClassGlobal);
            scope.IsClr = false;
            scope.PropertyStore = new DictionaryPropertyStore(scope);

            scope.DefineProperty(Id.@null, JsNull.Instance, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.Function, FunctionClass, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.Object, ObjectClass, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.Array, ArrayClass, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.Boolean, BooleanClass, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.Date, DateClass, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.Error, ErrorClass, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.EvalError, EvalErrorClass, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.RangeError, RangeErrorClass, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.ReferenceError, ReferenceErrorClass, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.SyntaxError, SyntaxErrorClass, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.TypeError, TypeErrorClass, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.URIError, URIErrorClass, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.Number, NumberClass, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.RegExp, RegExpClass, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.String, StringClass, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.Math, MathClass, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.NaN, NumberClass.GetProperty(Id.NaN), PropertyAttributes.DontEnum); // 15.1.1.1
            scope.DefineProperty(Id.Infinity, NumberClass.GetProperty(Id.POSITIVE_INFINITY), PropertyAttributes.DontEnum); // 15.1.1.2
            scope.DefineProperty(Id.undefined, JsUndefined.Instance, PropertyAttributes.DontEnum); // 15.1.1.3
            scope.DefineProperty(JsNames.This, scope, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.eval, GlobalFunctions.Eval, 1, PropertyAttributes.DontEnum); // 15.1.2.1
            scope.DefineProperty(Id.parseInt, GlobalFunctions.ParseInt, 1, PropertyAttributes.DontEnum); // 15.1.2.2
            scope.DefineProperty(Id.parseFloat, GlobalFunctions.ParseFloat, 1, PropertyAttributes.DontEnum); // 15.1.2.3
            scope.DefineProperty(Id.isNaN, GlobalFunctions.IsNaN, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.isFinite, GlobalFunctions.IsFinite, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.decodeURI, GlobalFunctions.DecodeURI, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.encodeURI, GlobalFunctions.EncodeURI, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.decodeURIComponent, GlobalFunctions.DecodeURIComponent, 1, PropertyAttributes.DontEnum);
            scope.DefineProperty(Id.encodeURIComponent, GlobalFunctions.EncodeURIComponent, 1, PropertyAttributes.DontEnum);

            return scope;
        }

        private static class GlobalFunctions
        {
            /// <summary>
            /// 15.1.2.1
            /// </summary>
            public static object Eval(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                if (arguments.Length == 0)
                    return JsNull.Instance;
                if (arguments[0].GetJsType() != JsType.String)
                    return arguments[0];

                try
                {
                    return runtime.Global.Engine.Execute(arguments[0].ToString(), false);
                }
                catch (JsException e)
                {
                    if (e.Type == JsErrorType.SyntaxError)
                        throw;

                    throw new JsException(JsErrorType.EvalError, e.Message);
                }
                catch (Exception e)
                {
                    throw new JsException(JsErrorType.EvalError, e.Message);
                }
            }

            /// <summary>
            /// 15.1.2.2
            /// </summary>
            public static object ParseInt(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
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
            public static object ParseFloat(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
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
            public static object IsNaN(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
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
            public static object IsFinite(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
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

            public static object DecodeURI(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                if (arguments.Length < 1 || JsValue.IsUndefined(arguments[0]))
                    return String.Empty;

                return Uri.UnescapeDataString(JsValue.ToString(arguments[0]).Replace("+", " "));
            }

            public static object EncodeURI(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
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

            public static object DecodeURIComponent(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                if (arguments.Length < 1 || JsValue.IsUndefined(arguments[0]))
                    return String.Empty;

                return Uri.UnescapeDataString(JsValue.ToString(arguments[0]).Replace("+", " "));
            }

            public static object EncodeURIComponent(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
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
