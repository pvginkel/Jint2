using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Jint.Runtime;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private JsObject CreateGlobalScope()
        {
            var scope = CreateObject();

            scope.SetClass(JsNames.ClassGlobal);
            scope.SetIsClr(false);
            scope.PropertyStore = new GlobalScopePropertyStore(scope);

            DefineProperty(scope, "null", JsNull.Instance, PropertyAttributes.DontEnum);
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
            DefineProperty(scope, "undefined", JsUndefined.Instance, PropertyAttributes.DontEnum); // 15.1.1.3
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

            public override bool TryGetProperty(JsInstance index, out JsInstance result)
            {
                return TryGetProperty(_global.ResolveIdentifier(index.ToString()), out result);
            }

            public override bool TryGetProperty(int index, out JsInstance result)
            {
                var descriptor = Owner.GetDescriptor(index);
                if (descriptor != null)
                    result = descriptor.Get(Owner);
                else
                    result = _global.Engine.ResolveUndefined(_global.GetIdentifier(index), null);

                return true;
            }
        }

        private static class GlobalFunctions
        {
            public static JsInstance ToBoolean(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsBoolean.Create(Convert.ToBoolean(arguments[0].Value));
            }

            public static JsInstance ToByte(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Convert.ToByte(arguments[0].Value));
            }

            public static JsInstance ToChar(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Convert.ToChar(arguments[0].Value));
            }

            public static JsInstance ToDateTime(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return runtime.Global.CreateDate((DateTime)arguments[0].Value);
            }

            public static JsInstance ToDecimal(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create((double)Convert.ToDecimal(arguments[0].Value));
            }

            public static JsInstance ToDouble(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Convert.ToDouble(arguments[0].Value));
            }

            public static JsInstance ToInt16(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Convert.ToInt16(arguments[0].Value));
            }

            public static JsInstance ToInt32(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Convert.ToInt32(arguments[0].Value));
            }

            public static JsInstance ToInt64(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Convert.ToInt64(arguments[0].Value));
            }

            public static JsInstance ToSByte(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Convert.ToSByte(arguments[0].Value));
            }

            public static JsInstance ToSingle(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Convert.ToSingle(arguments[0].Value));
            }

            public static JsInstance ToString(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsString.Create(@this.ToString());
            }

            public static JsInstance ToUInt16(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Convert.ToUInt16(arguments[0].Value));
            }

            public static JsInstance ToUInt32(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Convert.ToUInt32(arguments[0].Value));
            }

            public static JsInstance ToUInt64(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Convert.ToUInt64(arguments[0].Value));
            }

            /// <summary>
            /// 15.1.2.1
            /// </summary>
            public static JsInstance Eval(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (JsNames.ClassString != arguments[0].Class)
                    return arguments[0];

                return runtime.Global.Engine.Eval(arguments);
            }

            /// <summary>
            /// 15.1.2.2
            /// </summary>
            public static JsInstance ParseInt(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length < 1 || JsInstance.IsUndefined(arguments[0]))
                    return JsUndefined.Instance;

                //in case of an enum, just cast it to an integer
                if (arguments[0].IsClr && arguments[0].Value.GetType().IsEnum)
                    return JsNumber.Create((int)arguments[0].Value);

                string number = arguments[0].ToString().Trim();
                int sign = 1;
                int radix = 10;

                if (number == String.Empty)
                    return JsNumber.NaN;

                if (number.StartsWith("-"))
                {
                    number = number.Substring(1);
                    sign = -1;
                }
                else if (number.StartsWith("+"))
                {
                    number = number.Substring(1);
                }

                if (arguments.Length >= 2)
                {
                    if (!JsInstance.IsUndefined(arguments[1]) && !0.Equals(arguments[1]))
                    {
                        radix = Convert.ToInt32(arguments[1].Value);
                    }
                }

                if (radix == 0)
                {
                    radix = 10;
                }
                else if (radix < 2 || radix > 36)
                {
                    return JsNumber.NaN;
                }

                if (number.ToLower().StartsWith("0x"))
                {
                    radix = 16;
                }

                try
                {
                    if (radix == 10)
                    {
                        // most common case
                        double result;
                        if (double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                        {
                            // parseInt(12.42) == 42
                            return JsNumber.Create(sign * Math.Floor(result));
                        }
                        else
                        {
                            return JsNumber.NaN;
                        }
                    }
                    else
                    {
                        return JsNumber.Create(sign * Convert.ToInt32(number, radix));
                    }
                }
                catch
                {
                    return JsNumber.NaN;
                }
            }

            /// <summary>
            /// 15.1.2.3
            /// </summary>
            public static JsInstance ParseFloat(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length < 1 || JsInstance.IsUndefined(arguments[0]))
                {
                    return JsUndefined.Instance;
                }

                string number = arguments[0].ToString().Trim();
                // the parseFloat function should stop parsing when it encounters an unalowed char
                Regex regexp = new Regex(@"^[\+\-\d\.e]*", RegexOptions.IgnoreCase);

                Match match = regexp.Match(number);

                double result;
                if (match.Success && double.TryParse(match.Value, NumberStyles.Float, new CultureInfo("en-US"), out result))
                {
                    return JsNumber.Create(result);
                }
                else
                {
                    return JsNumber.NaN;
                }
            }

            /// <summary>
            /// 15.1.2.4
            /// </summary>
            public static JsInstance IsNaN(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length < 1)
                {
                    return JsBoolean.Create(false);
                }

                return JsBoolean.Create(double.NaN.Equals(arguments[0].ToNumber()));
            }

            /// <summary>
            /// 15.1.2.5
            /// </summary>
            public static JsInstance IsFinite(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length < 1 || JsInstance.IsUndefined(arguments[0]))
                    return JsBoolean.False;

                var value = arguments[0];
                return JsBoolean.Create(
                    value != JsNumber.NaN &&
                    value != JsNumber.PositiveInfinity &&
                    value != JsNumber.NegativeInfinity
                );
            }

            public static JsInstance DecodeURI(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length < 1 || JsInstance.IsUndefined(arguments[0]))
                    return JsString.Create();

                return JsString.Create(Uri.UnescapeDataString(arguments[0].ToString().Replace("+", " ")));
            }

            private static readonly char[] ReservedEncoded = new char[] { ';', ',', '/', '?', ':', '@', '&', '=', '+', '$', '#' };
            private static readonly char[] ReservedEncodedComponent = new char[] { '-', '_', '.', '!', '~', '*', '\'', '(', ')', '[', ']' };

            public static JsInstance EncodeURI(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length < 1 || JsInstance.IsUndefined(arguments[0]))
                    return JsString.Create();

                string encoded = Uri.EscapeDataString(arguments[0].ToString());

                foreach (char c in ReservedEncoded)
                {
                    encoded = encoded.Replace(Uri.EscapeDataString(c.ToString()), c.ToString());
                }

                foreach (char c in ReservedEncodedComponent)
                {
                    encoded = encoded.Replace(Uri.EscapeDataString(c.ToString()), c.ToString());
                }

                return JsString.Create(encoded.ToUpper());
            }

            public static JsInstance DecodeURIComponent(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length < 1 || JsInstance.IsUndefined(arguments[0]))
                    return JsString.Create();

                return JsString.Create(Uri.UnescapeDataString(arguments[0].ToString().Replace("+", " ")));
            }

            public static JsInstance EncodeURIComponent(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length < 1 || JsInstance.IsUndefined(arguments[0]))
                    return JsString.Create();

                string encoded = Uri.EscapeDataString(arguments[0].ToString());

                foreach (char c in ReservedEncodedComponent)
                {
                    encoded = encoded.Replace(Uri.EscapeDataString(c.ToString()), c.ToString().ToUpper());
                }

                return JsString.Create(encoded);
            }
        }
    }
}
