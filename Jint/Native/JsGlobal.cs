﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Jint.Runtime;

namespace Jint.Native
{
    [Serializable]
    public partial class JsGlobal
    {
        private readonly Dictionary<string, int> _identifiersByName = new Dictionary<string, int>();
        private readonly Dictionary<int, string> _identifiersByIndex = new Dictionary<int, string>();

        public IJintBackend Backend { get; set; }

        public Options Options { get; set; }

        public JsObject GlobalScope { get; private set; }

        internal JsObject PrototypeSink { get; private set; }

        internal Random Random { get; private set; }

        public JsGlobal(JintRuntime runtime, IJintBackend backend, Options options)
        {
            Id.SeedGlobal(this);

            PrototypeSink = new Sink(this);

            Options = options;
            Backend = backend;

            // The Random instance is used by Math to generate random numbers.
            Random = new Random();

            BuildEnvironment();

            GlobalScope = new Scope(this);

            Marshaller = new Marshaller(runtime, this);
            Marshaller.Initialize();
        }

        public JsFunction ObjectClass { get; private set; }
        public JsFunction FunctionClass { get; private set; }
        public JsFunction ArrayClass { get; private set; }
        public JsFunction BooleanClass { get; private set; }
        public JsFunction DateClass { get; private set; }
        public JsFunction ErrorClass { get; private set; }
        public JsFunction EvalErrorClass { get; private set; }
        public JsFunction RangeErrorClass { get; private set; }
        public JsFunction ReferenceErrorClass { get; private set; }
        public JsFunction SyntaxErrorClass { get; private set; }
        public JsFunction TypeErrorClass { get; private set; }
        public JsFunction URIErrorClass { get; private set; }

        public JsObject MathClass { get; private set; }
        public JsFunction NumberClass { get; private set; }
        public JsFunction RegExpClass { get; private set; }
        public JsFunction StringClass { get; private set; }
        public Marshaller Marshaller { get; private set; }

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
                    return CreateDate((DateTime)value);

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
                    return CreateObject(value, ObjectClass.Prototype);

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

        internal int ResolveIdentifier(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            // We don't cache indexes. These are passed verbatim.

            int result;
            if (int.TryParse(name, out result) && result >= 0)
                return result;

            if (!_identifiersByName.TryGetValue(name, out result))
            {
                result = _identifiersByName.Count + 1;
                _identifiersByName.Add(name, result);
                _identifiersByIndex.Add(result, name);
            }

            // Identifiers are negative so that they don't conflict with
            // array indexes.

            return -result;
        }

        internal string GetIdentifier(int index)
        {
            if (index >= 0)
                return index.ToString(CultureInfo.InvariantCulture);

            return _identifiersByIndex[-index];
        }
    }
}
