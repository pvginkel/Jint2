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
        partial class Scope
        {
            private static class Functions
            {
                public static JsInstance ToBoolean(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
                {
                    return JsBoolean.Create(Convert.ToBoolean(arguments[0].Value));
                }

                public static JsInstance ToByte(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
                {
                    return JsNumber.Create(Convert.ToByte(arguments[0].Value));
                }

                public static JsInstance ToChar(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
                {
                    return JsNumber.Create(Convert.ToChar(arguments[0].Value));
                }

                public static JsInstance ToDateTime(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
                {
                    return runtime.Global.CreateDate((DateTime)arguments[0].Value);
                }

                public static JsInstance ToDecimal(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
                {
                    return JsNumber.Create((double)Convert.ToDecimal(arguments[0].Value));
                }

                public static JsInstance ToDouble(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
                {
                    return JsNumber.Create(Convert.ToDouble(arguments[0].Value));
                }

                public static JsInstance ToInt16(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
                {
                    return JsNumber.Create(Convert.ToInt16(arguments[0].Value));
                }

                public static JsInstance ToInt32(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
                {
                    return JsNumber.Create(Convert.ToInt32(arguments[0].Value));
                }

                public static JsInstance ToInt64(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
                {
                    return JsNumber.Create(Convert.ToInt64(arguments[0].Value));
                }

                public static JsInstance ToSByte(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
                {
                    return JsNumber.Create(Convert.ToSByte(arguments[0].Value));
                }

                public static JsInstance ToSingle(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
                {
                    return JsNumber.Create(Convert.ToSingle(arguments[0].Value));
                }

                public static JsInstance ToString(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
                {
                    return JsString.Create(@this.ToString());
                }

                public static JsInstance ToUInt16(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
                {
                    return JsNumber.Create(Convert.ToUInt16(arguments[0].Value));
                }

                public static JsInstance ToUInt32(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
                {
                    return JsNumber.Create(Convert.ToUInt32(arguments[0].Value));
                }

                public static JsInstance ToUInt64(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
                {
                    return JsNumber.Create(Convert.ToUInt64(arguments[0].Value));
                }

                /// <summary>
                /// 15.1.2.1
                /// </summary>
                public static JsInstance Eval(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
                {
                    if (ClassString != arguments[0].Class)
                        return arguments[0];

                    return runtime.Global.Backend.Eval(arguments);
                }

                /// <summary>
                /// 15.1.2.2
                /// </summary>
                public static JsInstance ParseInt(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
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
                public static JsInstance ParseFloat(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
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
                public static JsInstance IsNaN(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
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
                public static JsInstance IsFinite(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
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

                public static JsInstance DecodeURI(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
                {
                    if (arguments.Length < 1 || JsInstance.IsUndefined(arguments[0]))
                        return JsString.Create();

                    return JsString.Create(Uri.UnescapeDataString(arguments[0].ToString().Replace("+", " ")));
                }

                private static readonly char[] ReservedEncoded = new char[] { ';', ',', '/', '?', ':', '@', '&', '=', '+', '$', '#' };
                private static readonly char[] ReservedEncodedComponent = new char[] { '-', '_', '.', '!', '~', '*', '\'', '(', ')', '[', ']' };

                public static JsInstance EncodeURI(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
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

                public static JsInstance DecodeURIComponent(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
                {
                    if (arguments.Length < 1 || JsInstance.IsUndefined(arguments[0]))
                        return JsString.Create();

                    return JsString.Create(Uri.UnescapeDataString(arguments[0].ToString().Replace("+", " ")));
                }

                public static JsInstance EncodeURIComponent(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
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
}
