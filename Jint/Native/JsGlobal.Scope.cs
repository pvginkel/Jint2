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
            scope.SetIsClr(false);
            scope.PropertyStore = new GlobalScopePropertyStore(scope);

            DefineProperty(scope, "null", JsBox.Null, PropertyAttributes.DontEnum);
            DefineProperty(scope, "Function", FunctionClass, PropertyAttributes.DontEnum);
            DefineProperty(scope, "Object", ObjectClass, PropertyAttributes.DontEnum);
            DefineProperty(scope, "Array", ArrayClass, PropertyAttributes.DontEnum);
            DefineProperty(scope, "Boolean", BooleanClass, PropertyAttributes.DontEnum);
            DefineProperty(scope, "Date", DateClass, PropertyAttributes.DontEnum);
            DefineProperty(scope, "Error", ErrorClass, PropertyAttributes.DontEnum);
            DefineProperty(scope, "EvalError", EvalErrorClass, PropertyAttributes.DontEnum);
            DefineProperty(scope, "RangeError", RangeErrorClass, PropertyAttributes.DontEnum);
            DefineProperty(scope, "ReferenceError", ReferenceErrorClass, PropertyAttributes.DontEnum);
            DefineProperty(scope, "SyntaxError", SyntaxErrorClass, PropertyAttributes.DontEnum);
            DefineProperty(scope, "TypeError", TypeErrorClass, PropertyAttributes.DontEnum);
            DefineProperty(scope, "URIError", URIErrorClass, PropertyAttributes.DontEnum);
            DefineProperty(scope, "Number", NumberClass, PropertyAttributes.DontEnum);
            DefineProperty(scope, "RegExp", RegExpClass, PropertyAttributes.DontEnum);
            DefineProperty(scope, "String", StringClass, PropertyAttributes.DontEnum);
            DefineProperty(scope, "Math", MathClass, PropertyAttributes.DontEnum);
            DefineProperty(scope, "NaN", NumberClass.GetProperty(Id.NaN), PropertyAttributes.DontEnum); // 15.1.1.1
            DefineProperty(scope, "Infinity", NumberClass.GetProperty(Id.POSITIVE_INFINITY), PropertyAttributes.DontEnum); // 15.1.1.2
            DefineProperty(scope, "undefined", JsBox.Undefined, PropertyAttributes.DontEnum); // 15.1.1.3
            DefineProperty(scope, JsNames.This, scope, PropertyAttributes.DontEnum);
            DefineFunction(scope, "ToBoolean", GlobalFunctions.ToBoolean, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "ToByte", GlobalFunctions.ToByte, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "ToChar", GlobalFunctions.ToChar, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "ToDateTime", GlobalFunctions.ToDateTime, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "ToDecimal", GlobalFunctions.ToDecimal, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "ToDouble", GlobalFunctions.ToDouble, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "ToInt16", GlobalFunctions.ToInt16, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "ToInt32", GlobalFunctions.ToInt32, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "ToInt64", GlobalFunctions.ToInt64, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "ToSByte", GlobalFunctions.ToSByte, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "ToSingle", GlobalFunctions.ToSingle, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "ToString", GlobalFunctions.ToString, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "ToUInt16", GlobalFunctions.ToUInt16, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "ToUInt32", GlobalFunctions.ToUInt32, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "ToUInt64", GlobalFunctions.ToUInt64, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "eval", GlobalFunctions.Eval, 1, PropertyAttributes.DontEnum); // 15.1.2.1
            DefineFunction(scope, "parseInt", GlobalFunctions.ParseInt, 1, PropertyAttributes.DontEnum); // 15.1.2.2
            DefineFunction(scope, "parseFloat", GlobalFunctions.ParseFloat, 1, PropertyAttributes.DontEnum); // 15.1.2.3
            DefineFunction(scope, "isNaN", GlobalFunctions.IsNaN, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "isFinite", GlobalFunctions.IsFinite, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "decodeURI", GlobalFunctions.DecodeURI, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "encodeURI", GlobalFunctions.EncodeURI, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "decodeURIComponent", GlobalFunctions.DecodeURIComponent, 1, PropertyAttributes.DontEnum);
            DefineFunction(scope, "encodeURIComponent", GlobalFunctions.EncodeURIComponent, 1, PropertyAttributes.DontEnum);

            return scope;
        }

        // If we're the global scope, perform special handling on JsUndefined.
        private class GlobalScopePropertyStore : DictionaryPropertyStore
        {
            private readonly JsGlobal _global;

            public GlobalScopePropertyStore(JsObject owner)
                : base(owner)
            {
                _global = owner.Global;
            }

            public override bool TryGetProperty(JsBox index, out JsBox result)
            {
                return TryGetProperty(_global.ResolveIdentifier(index.ToString()), out result);
            }

            public override bool TryGetProperty(int index, out JsBox result)
            {
                var descriptor = Owner.GetDescriptor(index);
                if (descriptor != null)
                    result = descriptor.Get(JsBox.CreateObject(Owner));
                else
                    result = _global.Engine.ResolveUndefined(_global.GetIdentifier(index), null);

                return true;
            }
        }

        private static class GlobalFunctions
        {
            public static JsBox ToBoolean(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsBoolean.Box(Convert.ToBoolean(arguments[0].ToInstance().Value));
            }

            public static JsBox ToByte(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Convert.ToByte(arguments[0].ToInstance().Value));
            }

            public static JsBox ToChar(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Convert.ToChar(arguments[0].ToInstance().Value));
            }

            public static JsBox ToDateTime(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsBox.CreateObject(
                    runtime.Global.CreateDate((DateTime)arguments[0].ToInstance().Value)
                );
            }

            public static JsBox ToDecimal(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box((double)Convert.ToDecimal(arguments[0].ToInstance().Value));
            }

            public static JsBox ToDouble(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Convert.ToDouble(arguments[0].ToInstance().Value));
            }

            public static JsBox ToInt16(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Convert.ToInt16(arguments[0].ToInstance().Value));
            }

            public static JsBox ToInt32(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Convert.ToInt32(arguments[0].ToInstance().Value));
            }

            public static JsBox ToInt64(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Convert.ToInt64(arguments[0].ToInstance().Value));
            }

            public static JsBox ToSByte(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Convert.ToSByte(arguments[0].ToInstance().Value));
            }

            public static JsBox ToSingle(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Convert.ToSingle(arguments[0].ToInstance().Value));
            }

            public static JsBox ToString(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsString.Box(@this.ToString());
            }

            public static JsBox ToUInt16(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Convert.ToUInt16(arguments[0].ToInstance().Value));
            }

            public static JsBox ToUInt32(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Convert.ToUInt32(arguments[0].ToInstance().Value));
            }

            public static JsBox ToUInt64(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Convert.ToUInt64(arguments[0].ToInstance().Value));
            }

            /// <summary>
            /// 15.1.2.1
            /// </summary>
            public static JsBox Eval(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (JsNames.ClassString != arguments[0].GetClass())
                    return arguments[0];

                return runtime.Global.Engine.Eval(arguments);
            }

            /// <summary>
            /// 15.1.2.2
            /// </summary>
            public static JsBox ParseInt(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length < 1 || arguments[0].IsUndefined)
                    return JsBox.Undefined;

                // In case of an enum, just cast it to an integer
                if (arguments[0].IsClr)
                {
                    var value = arguments[0].ToInstance().Value;
                    if (value.GetType().IsEnum)
                        return JsNumber.Box((int)value);
                }

                string number = arguments[0].ToString().Trim();
                int sign = 1;
                int radix = 10;

                if (number == String.Empty)
                    return JsBox.NaN;

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
                    !arguments[1].IsUndefined &&
                    arguments[1].ToNumber() != 0
                )
                    radix = (int)arguments[1].ToNumber();

                if (radix == 0)
                    radix = 10;
                else if (radix < 2 || radix > 36)
                    return JsBox.NaN;

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
                            return JsNumber.Box(sign * Math.Floor(result));
                        }
                        else
                        {
                            return JsBox.NaN;
                        }
                    }
                    else
                    {
                        return JsNumber.Box(sign * Convert.ToInt32(number, radix));
                    }
                }
                catch
                {
                    return JsBox.NaN;
                }
            }

            /// <summary>
            /// 15.1.2.3
            /// </summary>
            public static JsBox ParseFloat(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length < 1 || arguments[0].IsUndefined)
                    return JsBox.Undefined;

                string number = arguments[0].ToString().Trim();
                // the parseFloat function should stop parsing when it encounters an disallowed char
                Regex regexp = new Regex(@"^[\+\-\d\.e]*", RegexOptions.IgnoreCase);

                Match match = regexp.Match(number);

                double result;
                if (match.Success && double.TryParse(match.Value, NumberStyles.Float, new CultureInfo("en-US"), out result))
                    return JsNumber.Box(result);
                else
                    return JsBox.NaN;
            }

            /// <summary>
            /// 15.1.2.4
            /// </summary>
            public static JsBox IsNaN(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length < 1)
                {
                    return JsBoolean.Box(false);
                }

                return JsBoolean.Box(double.NaN.Equals(arguments[0].ToNumber()));
            }

            /// <summary>
            /// 15.1.2.5
            /// </summary>
            public static JsBox IsFinite(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length < 1 || arguments[0].IsUndefined)
                    return JsBox.False;

                var value = arguments[0].ToNumber();

                return JsBoolean.Box(
                    !Double.IsNaN(value) &&
                    !Double.IsPositiveInfinity(value) &&
                    !Double.IsNegativeInfinity(value)
                );
            }

            public static JsBox DecodeURI(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length < 1 || arguments[0].IsUndefined)
                    return JsString.Box();

                return JsString.Box(Uri.UnescapeDataString(arguments[0].ToString().Replace("+", " ")));
            }

            public static JsBox EncodeURI(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length < 1 || arguments[0].IsUndefined)
                    return JsString.Box();

                string encoded = Uri.EscapeDataString(arguments[0].ToString());

                foreach (char c in ReservedEncoded)
                {
                    encoded = encoded.Replace(Uri.EscapeDataString(c.ToString()), c.ToString());
                }

                foreach (char c in ReservedEncodedComponent)
                {
                    encoded = encoded.Replace(Uri.EscapeDataString(c.ToString()), c.ToString());
                }

                return JsString.Box(encoded.ToUpper());
            }

            public static JsBox DecodeURIComponent(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length < 1 || arguments[0].IsUndefined)
                    return JsString.Box();

                return JsString.Box(Uri.UnescapeDataString(arguments[0].ToString().Replace("+", " ")));
            }

            public static JsBox EncodeURIComponent(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length < 1 || arguments[0].IsUndefined)
                    return JsString.Box();

                string encoded = Uri.EscapeDataString(arguments[0].ToString());

                foreach (char c in ReservedEncodedComponent)
                {
                    encoded = encoded.Replace(Uri.EscapeDataString(c.ToString()), c.ToString().ToUpper());
                }

                return JsString.Box(encoded);
            }
        }
    }
}
