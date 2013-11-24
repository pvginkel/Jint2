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
        private class Scope : JsObject
        {
            public Scope(JsGlobal global)
                : base(global)
            {
                this["null"] = JsNull.Instance;
                GetDescriptor("null").Enumerable = false;

                this["Function"] = global.FunctionClass;
                GetDescriptor("Function").Enumerable = false;
                this["Object"] = global.ObjectClass;
                GetDescriptor("Object").Enumerable = false;

                this["Array"] = global.ArrayClass;
                GetDescriptor("Array").Enumerable = false;
                this["Boolean"] = global.BooleanClass;
                GetDescriptor("Boolean").Enumerable = false;
                this["Date"] = global.DateClass;
                GetDescriptor("Date").Enumerable = false;

                this["Error"] = global.ErrorClass;
                GetDescriptor("Error").Enumerable = false;
                this["EvalError"] = global.EvalErrorClass;
                GetDescriptor("EvalError").Enumerable = false;
                this["RangeError"] = global.RangeErrorClass;
                GetDescriptor("RangeError").Enumerable = false;
                this["ReferenceError"] = global.ReferenceErrorClass;
                GetDescriptor("ReferenceError").Enumerable = false;
                this["SyntaxError"] = global.SyntaxErrorClass;
                GetDescriptor("SyntaxError").Enumerable = false;
                this["TypeError"] = global.TypeErrorClass;
                GetDescriptor("TypeError").Enumerable = false;
                this["URIError"] = global.URIErrorClass;
                GetDescriptor("URIError").Enumerable = false;

                this["Number"] = global.NumberClass;
                GetDescriptor("Number").Enumerable = false;
                this["RegExp"] = global.RegExpClass;
                GetDescriptor("RegExp").Enumerable = false;
                this["String"] = global.StringClass;
                GetDescriptor("String").Enumerable = false;
                this["Math"] = global.MathClass;
                GetDescriptor("Math").Enumerable = false;

                // 15.1 prototype of the global object varies on the implementation
                //Prototype = ObjectClass.PrototypeProperty;

                this["NaN"] = global.NumberClass["NaN"];  // 15.1.1.1
                GetDescriptor("NaN").Enumerable = false;
                this["Infinity"] = global.NumberClass["POSITIVE_INFINITY"]; // // 15.1.1.2
                GetDescriptor("Infinity").Enumerable = false;
                this["undefined"] = JsUndefined.Instance; // 15.1.1.3
                GetDescriptor("undefined").Enumerable = false;
                this[JsNames.This] = this;

                this["ToBoolean"] = global.FunctionClass.New(new Func<object, Boolean>(Convert.ToBoolean));
                this["ToByte"] = global.FunctionClass.New(new Func<object, Byte>(Convert.ToByte));
                this["ToChar"] = global.FunctionClass.New(new Func<object, Char>(Convert.ToChar));
                this["ToDateTime"] = global.FunctionClass.New(new Func<object, DateTime>(Convert.ToDateTime));
                this["ToDecimal"] = global.FunctionClass.New(new Func<object, Decimal>(Convert.ToDecimal));
                this["ToDouble"] = global.FunctionClass.New(new Func<object, Double>(Convert.ToDouble));
                this["ToInt16"] = global.FunctionClass.New(new Func<object, Int16>(Convert.ToInt16));
                this["ToInt32"] = global.FunctionClass.New(new Func<object, Int32>(Convert.ToInt32));
                this["ToInt64"] = global.FunctionClass.New(new Func<object, Int64>(Convert.ToInt64));
                this["ToSByte"] = global.FunctionClass.New(new Func<object, SByte>(Convert.ToSByte));
                this["ToSingle"] = global.FunctionClass.New(new Func<object, Single>(Convert.ToSingle));
                this["ToString"] = global.FunctionClass.New(new Func<object, String>(Convert.ToString));
                this["ToUInt16"] = global.FunctionClass.New(new Func<object, UInt16>(Convert.ToUInt16));
                this["ToUInt32"] = global.FunctionClass.New(new Func<object, UInt32>(Convert.ToUInt32));
                this["ToUInt64"] = global.FunctionClass.New(new Func<object, UInt64>(Convert.ToUInt64));

                // every embed function should have a prototype FunctionClass.PrototypeProperty - 15.
                this["eval"] = new JsFunctionWrapper(global, Eval, global.FunctionClass.Prototype); // 15.1.2.1
                GetDescriptor("eval").Enumerable = false;
                this["parseInt"] = new JsFunctionWrapper(global, ParseInt, global.FunctionClass.Prototype); // 15.1.2.2
                GetDescriptor("parseInt").Enumerable = false;
                this["parseFloat"] = new JsFunctionWrapper(global, ParseFloat, global.FunctionClass.Prototype); // 15.1.2.3
                GetDescriptor("parseFloat").Enumerable = false;
                this["isNaN"] = new JsFunctionWrapper(global, IsNaN, global.FunctionClass.Prototype);
                GetDescriptor("isNaN").Enumerable = false;
                this["isFinite"] = new JsFunctionWrapper(global, IsFinite, global.FunctionClass.Prototype);
                GetDescriptor("isFinite").Enumerable = false;
                this["decodeURI"] = new JsFunctionWrapper(global, DecodeURI, global.FunctionClass.Prototype);
                GetDescriptor("decodeURI").Enumerable = false;
                this["encodeURI"] = new JsFunctionWrapper(global, EncodeURI, global.FunctionClass.Prototype);
                GetDescriptor("encodeURI").Enumerable = false;
                this["decodeURIComponent"] = new JsFunctionWrapper(global, DecodeURIComponent, global.FunctionClass.Prototype);
                GetDescriptor("decodeURIComponent").Enumerable = false;
                this["encodeURIComponent"] = new JsFunctionWrapper(global, EncodeURIComponent, global.FunctionClass.Prototype);
                GetDescriptor("encodeURIComponent").Enumerable = false;
            }

            public override string Class
            {
                get { return ClassGlobal; }
            }

            public override JsInstance this[string index]
            {
                get
                {
                    var descriptor = GetDescriptor(index);
                    if (descriptor != null)
                        return descriptor.Get(this);

                    // If we're the global scope, perform special handling on JsUndefined.

                    return Global.Backend.ResolveUndefined(index, null);
                }
                set
                {
                    base[index] = value;
                }
            }

            /// <summary>
            /// 15.1.2.1
            /// </summary>
            public JsInstance Eval(JsInstance[] arguments)
            {
                if (JsInstance.ClassString != arguments[0].Class)
                {
                    return arguments[0];
                }

                return Global.Backend.Eval(arguments);
            }

            /// <summary>
            /// 15.1.2.2
            /// </summary>
            public JsInstance ParseInt(JsInstance[] arguments)
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
            public JsInstance ParseFloat(JsInstance[] arguments)
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
            public JsInstance IsNaN(JsInstance[] arguments)
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
            protected JsInstance IsFinite(JsInstance[] arguments)
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

            protected JsInstance DecodeURI(JsInstance[] arguments)
            {
                if (arguments.Length < 1 || JsInstance.IsUndefined(arguments[0]))
                    return JsString.Create();

                return JsString.Create(Uri.UnescapeDataString(arguments[0].ToString().Replace("+", " ")));
            }

            private static readonly char[] ReservedEncoded = new char[] { ';', ',', '/', '?', ':', '@', '&', '=', '+', '$', '#' };
            private static readonly char[] ReservedEncodedComponent = new char[] { '-', '_', '.', '!', '~', '*', '\'', '(', ')', '[', ']' };

            protected JsInstance EncodeURI(JsInstance[] arguments)
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

            protected JsInstance DecodeURIComponent(JsInstance[] arguments)
            {
                if (arguments.Length < 1 || JsInstance.IsUndefined(arguments[0]))
                    return JsString.Create();

                return JsString.Create(Uri.UnescapeDataString(arguments[0].ToString().Replace("+", " ")));
            }

            protected JsInstance EncodeURIComponent(JsInstance[] arguments)
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
