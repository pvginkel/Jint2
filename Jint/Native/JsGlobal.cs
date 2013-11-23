using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Jint.Native
{
    [Serializable]
    public class JsGlobal : JsObject
    {
        /// <summary>
        /// Useful for eval()
        /// </summary>
        public IJintBackend Backend { get; set; }

        public Options Options { get; set; }

        public JsGlobal(IJintBackend backend, Options options)
            : base(JsNull.Instance)
        {
            Options = options;
            Backend = backend;

            this["null"] = JsNull.Instance;
            GetDescriptor("null").Enumerable = false;
            JsObject objectProrotype = new JsObject(JsNull.Instance);

            JsFunction functionPrototype = new JsFunctionWrapper(
                p => JsUndefined.Instance,
                objectProrotype
            );

            Marshaller = new Marshaller(this);

            #region Global Classes
            this["Function"] = FunctionClass = new JsFunctionConstructor(this, functionPrototype);
            GetDescriptor("Function").Enumerable = false;
            this["Object"] = ObjectClass = new JsObjectConstructor(this, functionPrototype, objectProrotype);
            GetDescriptor("Object").Enumerable = false;
            ObjectClass.InitPrototype(this);


            this["Array"] = ArrayClass = new JsArrayConstructor(this);
            GetDescriptor("Array").Enumerable = false;
            this["Boolean"] = BooleanClass = new JsBooleanConstructor(this);
            GetDescriptor("Boolean").Enumerable = false;
            this["Date"] = DateClass = new JsDateConstructor(this);
            GetDescriptor("Date").Enumerable = false;

            this["Error"] = ErrorClass = new JsErrorConstructor(this, "Error");
            GetDescriptor("Error").Enumerable = false;
            this["EvalError"] = EvalErrorClass = new JsErrorConstructor(this, "EvalError");
            GetDescriptor("EvalError").Enumerable = false;
            this["RangeError"] = RangeErrorClass = new JsErrorConstructor(this, "RangeError");
            GetDescriptor("RangeError").Enumerable = false;
            this["ReferenceError"] = ReferenceErrorClass = new JsErrorConstructor(this, "ReferenceError");
            GetDescriptor("ReferenceError").Enumerable = false;
            this["SyntaxError"] = SyntaxErrorClass = new JsErrorConstructor(this, "SyntaxError");
            GetDescriptor("SyntaxError").Enumerable = false;
            this["TypeError"] = TypeErrorClass = new JsErrorConstructor(this, "TypeError");
            GetDescriptor("TypeError").Enumerable = false;
            this["URIError"] = URIErrorClass = new JsErrorConstructor(this, "URIError");
            GetDescriptor("URIError").Enumerable = false;

            this["Number"] = NumberClass = new JsNumberConstructor(this);
            GetDescriptor("Number").Enumerable = false;
            this["RegExp"] = RegExpClass = new JsRegExpConstructor(this);
            GetDescriptor("RegExp").Enumerable = false;
            this["String"] = StringClass = new JsStringConstructor(this);
            GetDescriptor("String").Enumerable = false;
            this["Math"] = MathClass = new JsMathConstructor(this);
            GetDescriptor("Math").Enumerable = false;

            // 15.1 prototype of the global object varies on the implementation
            //Prototype = ObjectClass.PrototypeProperty;
            #endregion


            foreach (var constructor in new JsConstructor[]
            {
                FunctionClass,
                ObjectClass,
                ArrayClass,
                BooleanClass,
                DateClass,
                ErrorClass,
                EvalErrorClass,
                RangeErrorClass,
                ReferenceErrorClass,
                SyntaxErrorClass,
                TypeErrorClass,
                URIErrorClass,
                NumberClass,
                RegExpClass,
                StringClass
            })
            {
                constructor.InitPrototype(this);
            }

            #region Global Properties
            this["NaN"] = NumberClass["NaN"];  // 15.1.1.1
            GetDescriptor("NaN").Enumerable = false;
            this["Infinity"] = NumberClass["POSITIVE_INFINITY"]; // // 15.1.1.2
            GetDescriptor("Infinity").Enumerable = false;
            this["undefined"] = JsUndefined.Instance; // 15.1.1.3
            GetDescriptor("undefined").Enumerable = false;
            this[JsNames.This] = this;
            #endregion

            #region Global Functions
            // every embed function should have a prototype FunctionClass.PrototypeProperty - 15.
            this["eval"] = new JsFunctionWrapper(Eval, FunctionClass.PrototypeProperty); // 15.1.2.1
            GetDescriptor("eval").Enumerable = false;
            this["parseInt"] = new JsFunctionWrapper(ParseInt, FunctionClass.PrototypeProperty); // 15.1.2.2
            GetDescriptor("parseInt").Enumerable = false;
            this["parseFloat"] = new JsFunctionWrapper(ParseFloat, FunctionClass.PrototypeProperty); // 15.1.2.3
            GetDescriptor("parseFloat").Enumerable = false;
            this["isNaN"] = new JsFunctionWrapper(IsNaN, FunctionClass.PrototypeProperty);
            GetDescriptor("isNaN").Enumerable = false;
            this["isFinite"] = new JsFunctionWrapper(IsFinite, FunctionClass.PrototypeProperty);
            GetDescriptor("isFinite").Enumerable = false;
            this["decodeURI"] = new JsFunctionWrapper(DecodeURI, FunctionClass.PrototypeProperty);
            GetDescriptor("decodeURI").Enumerable = false;
            this["encodeURI"] = new JsFunctionWrapper(EncodeURI, FunctionClass.PrototypeProperty);
            GetDescriptor("encodeURI").Enumerable = false;
            this["decodeURIComponent"] = new JsFunctionWrapper(DecodeURIComponent, FunctionClass.PrototypeProperty);
            GetDescriptor("decodeURIComponent").Enumerable = false;
            this["encodeURIComponent"] = new JsFunctionWrapper(EncodeURIComponent, FunctionClass.PrototypeProperty);
            GetDescriptor("encodeURIComponent").Enumerable = false;
            #endregion

            Marshaller.InitTypes();

        }

        public override string Class
        {
            get
            {
                return ClassGlobal;
            }
        }

        #region Global Functions

        public JsObjectConstructor ObjectClass { get; private set; }
        public JsFunctionConstructor FunctionClass { get; private set; }
        public JsArrayConstructor ArrayClass { get; private set; }
        public JsBooleanConstructor BooleanClass { get; private set; }
        public JsDateConstructor DateClass { get; private set; }
        public JsErrorConstructor ErrorClass { get; private set; }
        public JsErrorConstructor EvalErrorClass { get; private set; }
        public JsErrorConstructor RangeErrorClass { get; private set; }
        public JsErrorConstructor ReferenceErrorClass { get; private set; }
        public JsErrorConstructor SyntaxErrorClass { get; private set; }
        public JsErrorConstructor TypeErrorClass { get; private set; }
        public JsErrorConstructor URIErrorClass { get; private set; }

        public JsMathConstructor MathClass { get; private set; }
        public JsNumberConstructor NumberClass { get; private set; }
        public JsRegExpConstructor RegExpClass { get; private set; }
        public JsStringConstructor StringClass { get; private set; }
        public Marshaller Marshaller { get; private set; }

        /// <summary>
        /// 15.1.2.1
        /// </summary>
        public JsInstance Eval(JsInstance[] arguments)
        {
            if (JsInstance.ClassString != arguments[0].Class)
            {
                return arguments[0];
            }

            return Backend.Eval(arguments);
        }

        /// <summary>
        /// 15.1.2.2
        /// </summary>
        public JsInstance ParseInt(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] is JsUndefined)
            {
                return JsUndefined.Instance;
            }

            //in case of an enum, just cast it to an integer
            if (arguments[0].IsClr && arguments[0].Value.GetType().IsEnum)
                return NumberClass.New((int)arguments[0].Value);

            string number = arguments[0].ToString().Trim();
            int sign = 1;
            int radix = 10;

            if (number == String.Empty)
            {
                return this["NaN"];
            }

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
                if (!(arguments[1] is JsUndefined) && !0.Equals(arguments[1]))
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
                return this["NaN"];
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
                        return NumberClass.New(sign * Math.Floor(result));
                    }
                    else
                    {
                        return this["NaN"];
                    }
                }
                else
                {
                    return NumberClass.New(sign * Convert.ToInt32(number, radix));
                }
            }
            catch
            {
                return this["NaN"];
            }
        }

        /// <summary>
        /// 15.1.2.3
        /// </summary>
        public JsInstance ParseFloat(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] is JsUndefined)
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
                return NumberClass.New(result);
            }
            else
            {
                return this["NaN"];
            }
        }

        /// <summary>
        /// 15.1.2.4
        /// </summary>
        public JsInstance IsNaN(JsInstance[] arguments)
        {
            if (arguments.Length < 1)
            {
                return BooleanClass.New(false);
            }

            return BooleanClass.New(double.NaN.Equals(arguments[0].ToNumber()));
        }

        /// <summary>
        /// 15.1.2.5
        /// </summary>
        protected JsInstance IsFinite(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] is JsUndefined)
            {
                return BooleanClass.False;
            }

            var value = arguments[0];
            return BooleanClass.New((value != NumberClass["NaN"]
                && value != NumberClass["POSITIVE_INFINITY"]
                && value != NumberClass["NEGATIVE_INFINITY"])
            );
        }

        protected JsInstance DecodeURI(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] is JsUndefined)
            {
                return StringClass.New();
            }

            return StringClass.New(Uri.UnescapeDataString(arguments[0].ToString().Replace("+", " ")));
        }

        private static readonly char[] ReservedEncoded = new char[] { ';', ',', '/', '?', ':', '@', '&', '=', '+', '$', '#' };
        private static readonly char[] ReservedEncodedComponent = new char[] { '-', '_', '.', '!', '~', '*', '\'', '(', ')', '[', ']' };

        protected JsInstance EncodeURI(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] is JsUndefined)
            {
                return StringClass.New();
            }

            string encoded = Uri.EscapeDataString(arguments[0].ToString());

            foreach (char c in ReservedEncoded)
            {
                encoded = encoded.Replace(Uri.EscapeDataString(c.ToString()), c.ToString());
            }

            foreach (char c in ReservedEncodedComponent)
            {
                encoded = encoded.Replace(Uri.EscapeDataString(c.ToString()), c.ToString());
            }

            return StringClass.New(encoded.ToUpper());
        }

        protected JsInstance DecodeURIComponent(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] is JsUndefined)
            {
                return StringClass.New();
            }

            return StringClass.New(Uri.UnescapeDataString(arguments[0].ToString().Replace("+", " ")));
        }

        protected JsInstance EncodeURIComponent(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] is JsUndefined)
            {
                return StringClass.New();
            }

            string encoded = Uri.EscapeDataString(arguments[0].ToString());

            foreach (char c in ReservedEncodedComponent)
            {
                encoded = encoded.Replace(Uri.EscapeDataString(c.ToString()), c.ToString().ToUpper());
            }

            return StringClass.New(encoded);
        }

        #endregion
        [Obsolete]
        public JsObject Wrap(object value)
        {
            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.Boolean:
                    return BooleanClass.New((bool)value);

                case TypeCode.Char:
                case TypeCode.String:
                    return StringClass.New(Convert.ToString(value));

                case TypeCode.DateTime:
                    return DateClass.New((DateTime)value);

                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return NumberClass.New(Convert.ToDouble(value));

                case TypeCode.Object:
                    return ObjectClass.New(value);

                default:
                    throw new ArgumentNullException("value");
            }
        }

        public JsObject WrapClr(object value)
        {
            return (JsObject)Marshaller.MarshalClrValue(value);
        }

        public bool HasOption(Options options)
        {
            return (Options & options) == options;
        }

        public JsInstance NaN
        {
            get { return this["NaN"]; }
        }

        public override JsInstance this[string index]
        {
            get
            {
                var descriptor = GetDescriptor(index);
                if (descriptor != null)
                    return descriptor.Get(this);

                // If we're the global scope, perform special handling on JsUndefined.

                return Backend.ResolveUndefined(index, null);
            }
            set
            {
                base[index] = value;
            }
        }
    }
}
