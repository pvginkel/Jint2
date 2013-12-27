using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Jint.Native
{
    public partial class JsGlobal
    {
        private readonly JintRuntime _runtime;
        private readonly Dictionary<string, int> _identifiersByName = new Dictionary<string, int>();
        private readonly Dictionary<int, string> _identifiersByIndex = new Dictionary<int, string>();

        public JintEngine Engine { get; set; }

        public Options Options { get; set; }

        public JsObject GlobalScope { get; private set; }

        public JsSchema RootSchema { get; private set; }

        internal JsObject PrototypeSink { get; private set; }

        internal Random Random { get; private set; }

        public JsGlobal(JintRuntime runtime, JintEngine engine, Options options)
        {
            if (runtime == null)
                throw new ArgumentNullException("runtime");

            _runtime = runtime;

            Id.SeedGlobal(this);

            PrototypeSink = CreatePrototypeSink();
            RootSchema = new JsSchema();
            Options = options;
            Engine = engine;

            // The Random instance is used by Math to generate random numbers.
            Random = new Random();

            BuildEnvironment();

            GlobalScope = CreateGlobalScope();

            Marshaller = new Marshaller(runtime, this);
            Marshaller.Initialize();
        }

        public JsObject ObjectClass { get; private set; }
        public JsObject FunctionClass { get; private set; }
        public JsObject ArrayClass { get; private set; }
        public JsObject BooleanClass { get; private set; }
        public JsObject DateClass { get; private set; }
        public JsObject ErrorClass { get; private set; }
        public JsObject EvalErrorClass { get; private set; }
        public JsObject RangeErrorClass { get; private set; }
        public JsObject ReferenceErrorClass { get; private set; }
        public JsObject SyntaxErrorClass { get; private set; }
        public JsObject TypeErrorClass { get; private set; }
        public JsObject URIErrorClass { get; private set; }

        public JsObject MathClass { get; private set; }
        public JsObject NumberClass { get; private set; }
        public JsObject RegExpClass { get; private set; }
        public JsObject StringClass { get; private set; }
        public Marshaller Marshaller { get; private set; }

        public JsObject WrapClr(object value)
        {
            return (JsObject)Marshaller.MarshalClrValue(value);
        }

        public bool HasOption(Options options)
        {
            return (Options & options) == options;
        }

        internal JsObject GetPrototype(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            var @object = instance as JsObject;
            if (@object != null)
                return @object.Prototype;
            if (instance is string)
                return StringClass.Prototype;
            if (instance is double)
                return NumberClass.Prototype;
            if (instance is bool)
                return BooleanClass.Prototype;
            if (JsValue.IsUndefined(instance))
                return PrototypeSink;

            throw new InvalidOperationException();
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

        public object ExecuteFunction(JsObject function, object that, object[] arguments)
        {
            if (function == null)
                throw new ArgumentNullException("function");

            return function.Execute(_runtime, that, arguments);
        }
    }
}
