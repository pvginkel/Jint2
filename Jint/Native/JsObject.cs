using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    public sealed partial class JsObject : IComparable<JsObject>
    {
        internal static readonly ReadOnlyCollection<KeyValuePair<int, object>> EmptyKeyValues = new ReadOnlyCollection<KeyValuePair<int, object>>(new KeyValuePair<int, object>[0]);

        private IPropertyStore _propertyStore;
        private DictionaryPropertyStore _dictionaryPropertyStore;

        internal IPropertyStore PropertyStore
        {
            get { return _propertyStore; }
            set
            {
                _propertyStore = value;
                _dictionaryPropertyStore = value as DictionaryPropertyStore;
            }
        }

        internal JsDelegate Delegate { get; private set; }

        public object Value { get; set; }

        public JsGlobal Global { get; private set; }

        public bool IsClr { get; internal set; }

        public string Class { get; private set; }

        internal void SetClass(string @class)
        {
            if (@class == null)
                throw new ArgumentNullException("class");

            Class = @class;
        }

        /// <summary>
        /// ecma262 [[prototype]] property
        /// </summary>
        public JsObject Prototype { get; set; }

        internal JsObject(JsGlobal global, object value, JsObject prototype, JsDelegate @delegate)
        {
            if (global == null)
                throw new ArgumentNullException("global");

            Global = global;
            Value = value;
            Class = JsNames.ClassObject;
            Prototype = prototype ?? global.PrototypeSink;
            Delegate = @delegate;
        }

        private void EnsurePropertyStore()
        {
            if (PropertyStore == null)
                PropertyStore = new DictionaryPropertyStore(this);
        }

        public object ToPrimitive()
        {
            return ToPrimitive(PreferredType.None);
        }

        public object ToPrimitive(PreferredType preferredType)
        {
            return DefaultValue(preferredType);
        }

        public object DefaultValue(PreferredType hint)
        {
            if (hint == PreferredType.None)
            {
                // 8.6.2.6
                if (Class == JsNames.ClassDate)
                    hint = PreferredType.String;
            }

            object primitive;

            var toString = GetProperty(Id.toString);
            var valueOf = GetProperty(Id.valueOf);

            var first = hint == PreferredType.String ? toString : valueOf;
            var second = hint == PreferredType.String ? valueOf : toString;

            if (
                first != null &&
                TryExecuteToPrimitiveFunction(first, out primitive)
            )
                return primitive;

            if (
                second != null &&
                TryExecuteToPrimitiveFunction(second, out primitive)
            )
                return primitive;

            if (IsClr && Value != null)
            {
                if (!(Value is IComparable))
                    return Value.ToString();

                switch (Convert.GetTypeCode(Value))
                {
                    case TypeCode.Boolean:
                        return BooleanBoxes.Box((bool)Value);

                    case TypeCode.Char:
                    case TypeCode.String:
                    case TypeCode.Object:
                        return Value.ToString();

                    case TypeCode.DateTime:
                        return JsConvert.ToString((DateTime)Value);

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
                        return Convert.ToDouble(Value);

                    default:
                        return Value.ToString();
                }
            }

            throw new JsException(JsErrorType.TypeError, "Invalid type");
        }

        private bool TryExecuteToPrimitiveFunction(object function, out object primitive)
        {
            if (!JsValue.IsFunction(function))
            {
                primitive = null;
                return false;
            }

            var result = Global.ExecuteFunction(
                (JsObject)function,
                this,
                JsValue.EmptyArray
            );

            if (JsValue.IsPrimitive(result))
            {
                primitive = result;
                return true;
            }

            primitive = null;
            return false;
        }

        public bool ToBoolean()
        {
            if (Value != null && !(Value is IConvertible))
                return true;

            switch (Convert.GetTypeCode(Value))
            {
                case TypeCode.Boolean:
                    return (bool)Value;

                case TypeCode.Char:
                case TypeCode.String:
                    return JsConvert.ToBoolean((string)Value);

                case TypeCode.DateTime:
                    return JsConvert.ToBoolean(JsConvert.ToNumber((DateTime)Value));

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
                    return JsConvert.ToBoolean(Convert.ToDouble(Value));

                case TypeCode.Object:
                    return Convert.ToBoolean(Value);

                default:
                    return true;
            }
        }

        public double ToNumber()
        {
            return JsValue.ToNumber(ToPrimitive(PreferredType.Number));
        }

        public override string ToString()
        {
            return JsValue.ToString(ToPrimitive(PreferredType.String));
        }

        public bool IsPrototypeOf(JsObject target)
        {
            if (target == null)
                return false;
            if (target.IsPrototypeNull)
                return false;
            if (target.Prototype == this)
                return true;
            return IsPrototypeOf(target.Prototype);
        }

        public bool IsPrototypeNull
        {
            get
            {
                Debug.Assert(Prototype != null);
                return Prototype == Global.PrototypeSink;
            }
        }

        // 15.3.5.3
        public bool HasInstance(JsObject instance)
        {
            return
                instance != null &&
                Prototype.IsPrototypeOf(instance);
        }

        // 13.2.2
        public object Construct(JintRuntime runtime, params object[] arguments)
        {
            if (Delegate == null)
                throw new JsException(JsErrorType.TypeError, ToString() + " is not a function");

            var @this = Global.CreateObject((JsObject)GetProperty(Id.prototype));
            var boxedThis = (object)@this;

            var result = Delegate.Delegate(runtime, boxedThis, this, arguments) as JsObject;

            if (result != null)
                return result;

            return boxedThis;
        }

        public object Execute(JintRuntime runtime, object @this, params object[] arguments)
        {
            if (Delegate == null)
                throw new JsException(JsErrorType.TypeError, ToString() + " is not a function");

            return Delegate.Delegate(runtime, @this, this, arguments);
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : base.GetHashCode();
        }

        public int CompareTo(JsObject other)
        {
            return String.Compare(
                ToString(),
                other.ToString(),
                StringComparison.Ordinal
            );
        }
    }
}
