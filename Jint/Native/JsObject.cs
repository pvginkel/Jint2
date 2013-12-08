using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public sealed partial class JsObject : JsInstance
    {
        internal static readonly ReadOnlyCollection<KeyValuePair<int, JsBox>> EmptyKeyValues = new ReadOnlyCollection<KeyValuePair<int, JsBox>>(new KeyValuePair<int, JsBox>[0]);

        private object _value;
        private bool _isClr;
        private string _class;

        internal IPropertyStore PropertyStore { get; set; }
        internal JsDelegate Delegate { get; private set; }

        public override object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public JsGlobal Global { get; private set; }

        public override bool IsClr
        {
            get { return _isClr; }
        }

        internal void SetIsClr(bool isClr)
        {
            _isClr = isClr;
        }

        public override string Class
        {
            get { return _class; }
        }

        internal void SetClass(string @class)
        {
            if (@class == null)
                throw new ArgumentNullException("class");

            _class = @class;
        }

        public override JsType Type
        {
            get { return JsType.Object; }
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
            _value = value;
            _class = JsNames.ClassObject;
            Prototype = prototype ?? global.PrototypeSink;
            Delegate = @delegate;
        }

        private void EnsurePropertyStore()
        {
            if (PropertyStore == null)
                PropertyStore = new DictionaryPropertyStore(this);
        }

        public override JsBox ToPrimitive(PreferredType preferredType)
        {
            return DefaultValue(preferredType);
        }

        public JsBox DefaultValue(PreferredType hint)
        {
            if (hint == PreferredType.None)
            {
                // 8.6.2.6
                if (Class == JsNames.ClassDate)
                    hint = PreferredType.String;
            }

            JsBox primitive;

            var toString = GetProperty(Id.toString);
            var valueOf = GetProperty(Id.valueOf);

            var first = hint == PreferredType.String ? toString : valueOf;
            var second = hint == PreferredType.String ? valueOf : toString;

            if (
                first.IsValid &&
                TryExecuteToPrimitiveFunction(first, out primitive)
            )
                return primitive;

            if (
                second.IsValid &&
                TryExecuteToPrimitiveFunction(second, out primitive)
            )
                return primitive;

            if (IsClr && Value != null)
            {
                if (!(Value is IComparable))
                    return JsString.Box(Value.ToString());

                switch (Convert.GetTypeCode(Value))
                {
                    case TypeCode.Boolean:
                        return JsBox.CreateBoolean((bool)Value);

                    case TypeCode.Char:
                    case TypeCode.String:
                    case TypeCode.Object:
                        return JsString.Box(Value.ToString());

                    case TypeCode.DateTime:
                        return JsString.Box(JsConvert.ToString((DateTime)Value));

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
                        return JsBox.CreateNumber(Convert.ToDouble(Value));

                    default:
                        return JsString.Box(Value.ToString());
                }
            }

            throw new JsException(JsErrorType.TypeError, "Invalid type");
        }

        private bool TryExecuteToPrimitiveFunction(JsBox function, out JsBox primitive)
        {
            if (!function.IsFunction)
            {
                primitive = new JsBox();
                return false;
            }

            var result = Global.ExecuteFunction(
                (JsObject)function,
                JsBox.CreateObject(this),
                JsBox.EmptyArray,
                null
            );

            if (result.IsPrimitive)
            {
                primitive = result;
                return true;
            }

            primitive = new JsBox();
            return false;
        }

        public override bool ToBoolean()
        {
            if (
                Type == JsType.Object ||
                (Value != null && !(Value is IConvertible))
            )
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

        public override double ToNumber()
        {
            return ToPrimitive(PreferredType.Number).ToNumber();
        }

        public override string ToString()
        {
            return ToPrimitive(PreferredType.String).ToString();
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
        public JsBox Construct(JintRuntime runtime, JsBox[] arguments)
        {
            if (Delegate == null)
                throw new JsException(JsErrorType.TypeError, ToString() + " is not a function");

            var @this = Global.CreateObject((JsObject)GetProperty(Id.prototype));
            var boxedThis = JsBox.CreateObject(@this);

            var result = Delegate.Delegate(runtime, boxedThis, this, Delegate.Closure, arguments, null);

            if (result.IsObject)
                return result;

            return boxedThis;
        }

        public JsBox Execute(JintRuntime runtime, JsBox @this, JsBox[] arguments, JsBox[] genericArguments)
        {
            if (Delegate == null)
                throw new JsException(JsErrorType.TypeError, ToString() + " is not a function");

            return Delegate.Delegate(runtime, @this, this, Delegate.Closure, arguments, genericArguments);
        }
    }
}
