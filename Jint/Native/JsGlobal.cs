using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Jint.Native
{
    [Serializable]
    public partial class JsGlobal
    {
        /// <summary>
        /// Useful for eval()
        /// </summary>
        public IJintBackend Backend { get; set; }

        public Options Options { get; set; }

        public JsObject GlobalScope { get; private set; }

        internal JsObject PrototypeSink { get; private set; }

        public JsGlobal(IJintBackend backend, Options options)
        {
            PrototypeSink = new Sink(this);

            Options = options;
            Backend = backend;

            Marshaller = new Marshaller(this);

            var objectPrototype = new JsObject(this, PrototypeSink);

            var functionPrototype = new JsFunctionWrapper(
                this,
                p => JsUndefined.Instance,
                objectPrototype
            );

            // These two must be initialized special because they depend on
            // each other being available.

            FunctionClass = new JsFunctionConstructor(this, functionPrototype);
            ObjectClass = new JsObjectConstructor(this, objectPrototype);
            FunctionClass.InitPrototype();
            ObjectClass.InitPrototype();

            ArrayClass = new JsArrayConstructor(this);
            BooleanClass = new JsBooleanConstructor(this);
            DateClass = new JsDateConstructor(this);

            ErrorClass = new JsErrorConstructor(this, "Error");
            EvalErrorClass = new JsErrorConstructor(this, "EvalError");
            RangeErrorClass = new JsErrorConstructor(this, "RangeError");
            ReferenceErrorClass = new JsErrorConstructor(this, "ReferenceError");
            SyntaxErrorClass = new JsErrorConstructor(this, "SyntaxError");
            TypeErrorClass = new JsErrorConstructor(this, "TypeError");
            URIErrorClass = new JsErrorConstructor(this, "URIError");

            NumberClass = new JsNumberConstructor(this);
            RegExpClass = new JsRegExpConstructor(this);
            StringClass = new JsStringConstructor(this);
            MathClass = new JsMathConstructor(this);

            GlobalScope = new Scope(this);

            Marshaller.InitTypes();
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

        #endregion
        [Obsolete]
        public JsInstance Wrap(object value)
        {
            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.Boolean:
                    return JsBoolean.Create((bool)value);

                case TypeCode.Char:
                case TypeCode.String:
                    return JsString.Create(Convert.ToString(value));

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
                    return JsNumber.Create(Convert.ToDouble(value));

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

        internal JsObject GetPrototype(JsInstance instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            switch (instance.Type)
            {
                case JsType.Boolean: return BooleanClass.Prototype;
                case JsType.Null: throw new InvalidOperationException();
                case JsType.Number: return NumberClass.Prototype;
                case JsType.String: return StringClass.Prototype;
                case JsType.Undefined: return PrototypeSink;
                default: return ((JsObject)instance).Prototype;
            }
        }
    }
}
