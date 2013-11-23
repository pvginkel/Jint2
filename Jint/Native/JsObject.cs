using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class JsObject : JsDictionaryObject
    {
        public INativeIndexer Indexer { get; set; }

        public JsObject()
        {
        }

        public JsObject(object value, JsObject prototype)
            : base(prototype)
        {
            _value = value;
        }

        public JsObject(JsObject prototype)
            : base(prototype)
        {
        }

        public override bool IsClr
        {
            get
            {
                // if this instance holds a native value
                return Value != null;
            }
        }

        public override string Class
        {
            get { return ClassObject; }
        }

        public override JsType Type
        {
            get { return JsType.Object; }
        }

        private object _value;

        public override object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public override int GetHashCode()
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
        }

        public override JsInstance ToPrimitive(JsGlobal global, PrimitiveHint hint)
        {
            // 9.1
            if (
                this == JsNull.Instance ||
                this is JsUndefined ||
                this is JsBoolean ||
                this is JsNumber ||
                this is JsString
            )
                return this;

            if (hint == PrimitiveHint.None)
            {
                // 8.6.2.6
                if (this is JsDate)
                    hint = PrimitiveHint.String;
            }

            JsInstance primitive;

            var toString = GetDescriptor("toString");
            var valueOf = GetDescriptor("valueOf");

            var first = hint == PrimitiveHint.String ? toString : valueOf;
            var second = hint == PrimitiveHint.String ? valueOf : toString;

            if (
                first != null &&
                TryExecuteToPrimitiveFunction(global, first, out primitive)
            )
                return primitive;

            if (
                second != null &&
                TryExecuteToPrimitiveFunction(global, second, out primitive)
            )
                return primitive;

            if (IsClr && Value != null)
            {
                if (!(Value is IComparable))
                    return global.StringClass.New(Value.ToString());

                switch (Convert.GetTypeCode(Value))
                {
                    case TypeCode.Boolean:
                        return global.BooleanClass.New((bool)Value);

                    case TypeCode.Char:
                    case TypeCode.String:
                    case TypeCode.Object:
                        return global.StringClass.New(Value.ToString());

                    case TypeCode.DateTime:
                        return global.StringClass.New(JsDate.DateToString((DateTime)Value));

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
                        return global.NumberClass.New(Convert.ToDouble(Value));

                    default:
                        return global.StringClass.New(Value.ToString());
                }
            }

            throw new JsException(global.TypeErrorClass.New("Invalid type"));
        }

        private bool TryExecuteToPrimitiveFunction(JsGlobal global, Descriptor descriptor, out JsInstance primitive)
        {
            primitive = null;

            var function = descriptor.Get(this) as JsFunction;

            if (function == null)
                return false;

            var result = global.Backend.ExecuteFunction(
                function,
                this,
                Empty,
                null
            ).Result;

            if (result.IsPrimitive)
            {
                primitive = result;
                return true;
            }

            return false;
        }

        public override bool ToBoolean()
        {
            if (Value != null && !(Value is IConvertible))
                return true;

            if (Type == JsType.Object)
                return true;

            switch (Convert.GetTypeCode(Value))
            {
                case TypeCode.Boolean:
                    return (bool)Value;

                case TypeCode.Char:
                case TypeCode.String:
                    return JsString.StringToBoolean((string)Value);

                case TypeCode.DateTime:
                    return JsNumber.NumberToBoolean(JsDate.DateToDouble((DateTime)Value));

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
                    return JsNumber.NumberToBoolean(Convert.ToDouble(Value));

                case TypeCode.Object:
                    return Convert.ToBoolean(Value);

                default:
                    return true;
            }
        }

        public override double ToNumber()
        {
            if (Value == null)
                return 0;

            if (!(Value is IConvertible))
                return double.NaN;

            switch (Convert.GetTypeCode(Value))
            {
                case TypeCode.Boolean:
                    return JsBoolean.BooleanToNumber((bool)Value);
                case TypeCode.Char:
                case TypeCode.String:
                    return JsString.StringToNumber((string)Value);
                case TypeCode.DateTime:
                    return JsDate.DateToDouble((DateTime)Value);
                default:
                    return Convert.ToDouble(Value);
            }
        }

        public override string ToString()
        {
            if (_value == null)
            {
                return null;
            }

            if (_value is IConvertible)
                return Convert.ToString(Value);

            return _value.ToString();
        }

        public override JsInstance this[JsInstance key]
        {
            get
            {
                if (Indexer != null)
                    return Indexer.Get(this, key);

                return base[key];
            }
            set
            {
                if (Indexer != null)
                    Indexer.Set(this, key, value);
                else
                    base[key] = value;
            }
        }
    }
}
