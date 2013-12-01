using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    [Serializable]
#if !DEBUG
    [DebuggerTypeProxy(typeof(JsObjectDebugView))]
#endif
    public class JsObject : JsInstance, IEnumerable<KeyValuePair<string, JsInstance>>
    {
        internal static readonly ReadOnlyCollection<KeyValuePair<string, JsInstance>> EmptyKeyValues = new ReadOnlyCollection<KeyValuePair<string, JsInstance>>(new KeyValuePair<string, JsInstance>[0]);

        private int _length;
        private object _value;

        internal IPropertyStore PropertyStore { get; set; }

        public override object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Gets or sets the number of an actually stored properties.
        /// </summary>
        /// <remarks>
        /// This is a non ecma262 standard property.
        /// </remarks>
        public int Length
        {
            get { return _length; }
            internal set
            {
                _length = value;
                if (PropertyStore != null)
                    PropertyStore.SetLength(value);
            }
        }

        public JsGlobal Global { get; private set; }

        /// <summary>
        /// ecma262 [[prototype]] property
        /// </summary>
        public JsObject Prototype { get; internal set; }

        internal JsObject(JsGlobal global, object value, JsObject prototype)
        {
            if (global == null)
                throw new ArgumentNullException("global");

            Global = global;
            _value = value;
            Prototype = prototype ?? global.PrototypeSink;
        }

        public override bool IsClr
        {
            get
            {
                // if this instance holds a native value
                return _value != null;
            }
        }

        public override string Class
        {
            get { return JsNames.ClassObject; }
        }

        public override JsType Type
        {
            get { return JsType.Object; }
        }

        private void EnsurePropertyStore()
        {
            if (PropertyStore == null)
                PropertyStore = new DictionaryPropertyStore(this);
        }

        public override JsInstance ToPrimitive(PreferredType preferredType)
        {
            return DefaultValue(preferredType);
        }

        public JsInstance DefaultValue(PreferredType hint)
        {
            if (hint == PreferredType.None)
            {
                // 8.6.2.6
                if (this is JsDate)
                    hint = PreferredType.String;
            }

            JsInstance primitive;

            var toString = GetDescriptor("toString");
            var valueOf = GetDescriptor("valueOf");

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
                    return JsString.Create(Value.ToString());

                switch (Convert.GetTypeCode(Value))
                {
                    case TypeCode.Boolean:
                        return JsBoolean.Create((bool)Value);

                    case TypeCode.Char:
                    case TypeCode.String:
                    case TypeCode.Object:
                        return JsString.Create(Value.ToString());

                    case TypeCode.DateTime:
                        return JsString.Create(JsConvert.ToString((DateTime)Value));

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
                        return JsNumber.Create(Convert.ToDouble(Value));

                    default:
                        return JsString.Create(Value.ToString());
                }
            }

            throw new JsException(JsErrorType.TypeError, "Invalid type");
        }

        private bool TryExecuteToPrimitiveFunction(Descriptor descriptor, out JsInstance primitive)
        {
            primitive = null;

            var function = descriptor.Get(this) as JsFunction;

            if (function == null)
                return false;

            var result = Global.Backend.ExecuteFunction(
                function,
                this,
                JsInstance.EmptyArray,
                null
            );

            if (result.IsPrimitive)
            {
                primitive = result;
                return true;
            }

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

        public override string ToSource()
        {
            var function = this as JsFunction;
            if (function != null)
                return String.Format("function {0} () {{ /* js code */ }}", function.Name);

            return ToString();
        }

        /// <summary>
        /// Checks whether an object or it's [[prototype]] has the specified property.
        /// </summary>
        /// <param name="key">property name</param>
        /// <returns>true or false indicating check result</returns>
        /// <remarks>
        /// This implementation uses a HasOwnProperty method while walking a prototypes chain.
        /// </remarks>
        public bool HasProperty(string key)
        {
            var @object = this;

            while (true)
            {
                if (@object.HasOwnProperty(key))
                    return true;

                @object = @object.Prototype;

                if (@object.IsPrototypeNull)
                    return false;
            }
        }

        public bool HasProperty(JsInstance index)
        {
            var @object = this;

            while (true)
            {
                if (@object.HasOwnProperty(index))
                    return true;

                @object = @object.Prototype;

                if (@object.IsPrototypeNull)
                    return false;
            }
        }

        public bool HasOwnProperty(string index)
        {
            if (PropertyStore == null)
                return false;

            return PropertyStore.HasOwnProperty(index);
        }

        public bool HasOwnProperty(JsInstance index)
        {
            if (PropertyStore == null)
                return false;

            return PropertyStore.HasOwnProperty(index);
        }

        public Descriptor GetOwnDescriptor(string index)
        {
            if (PropertyStore == null)
                return null;

            return PropertyStore.GetOwnDescriptor(index);
        }

        public Descriptor GetOwnDescriptor(JsInstance index)
        {
            if (PropertyStore == null)
                return null;

            return PropertyStore.GetOwnDescriptor(index);
        }

        public Descriptor GetDescriptor(string index)
        {
            var result = GetOwnDescriptor(index);
            if (result != null)
                return result;

            if (IsPrototypeNull)
                return null;

            return Prototype.GetDescriptor(index);
        }

        public Descriptor GetDescriptor(JsInstance index)
        {
            var result = GetOwnDescriptor(index);
            if (result != null)
                return result;

            if (IsPrototypeNull)
                return null;

            return Prototype.GetDescriptor(index);
        }

        public bool TryGetDescriptor(JsInstance index, out Descriptor result)
        {
            result = GetDescriptor(index);
            return result != null;
        }

        public bool TryGetDescriptor(string index, out Descriptor result)
        {
            result = GetDescriptor(index);
            return result != null;
        }

        public bool TryGetProperty(JsInstance index, out JsInstance result)
        {
            if (PropertyStore != null)
                return PropertyStore.TryGetProperty(index, out result);

            result = null;
            return false;
        }

        public bool TryGetProperty(string index, out JsInstance result)
        {
            if (PropertyStore != null)
                return PropertyStore.TryGetProperty(index, out result);

            result = null;
            return false;
        }

        public JsInstance this[JsInstance index]
        {
            get { return GetProperty(index); }
            set { SetProperty(index, value); }
        }

        public JsInstance this[string index]
        {
            get { return GetProperty(index); }
            set { SetProperty(index, value); }
        }

        private JsInstance GetProperty(JsInstance index)
        {
            if (PropertyStore != null)
            {
                JsInstance result;
                if (PropertyStore.TryGetProperty(index, out result))
                    return result;
            }

            return GetPropertyCore(index.ToString());
        }

        private JsInstance GetProperty(string index)
        {
            if (PropertyStore != null)
            {
                JsInstance result;
                if (PropertyStore.TryGetProperty(index, out result))
                    return result;
            }

            return GetPropertyCore(index);
        }

        private JsInstance GetPropertyCore(string index)
        {
            Descriptor descriptor;

            if (index == "prototype")
            {
                descriptor = GetOwnDescriptor("prototype");
                if (descriptor != null)
                    return descriptor.Get(this);

                if (IsPrototypeNull)
                    return JsNull.Instance;

                return Prototype;
            }

            if (index == "__proto__")
            {
                if (IsPrototypeNull)
                    return JsNull.Instance;

                return Prototype;
            }

            descriptor = GetDescriptor(index);

            if (descriptor == null)
            {
                Trace.WriteLine(String.Format(
                    "Unresolved identifier {0} of {1}",
                    index,
                    GetType()
                ));
            }

            return
                descriptor != null
                ? descriptor.Get(this)
                : JsUndefined.Instance;
        }

        private void SetProperty(string index, JsInstance value)
        {
            EnsurePropertyStore();
            if (!PropertyStore.TrySetProperty(index, value))
                SetPropertyCore(index, value);
        }

        private void SetProperty(JsInstance index, JsInstance value)
        {
            EnsurePropertyStore();
            if (!PropertyStore.TrySetProperty(index, value))
                SetPropertyCore(index.ToString(), value);
        }

        private void SetPropertyCore(string index, JsInstance value)
        {
            if (index == "__proto__")
            {
                if (value.Type == JsType.Object)
                    Prototype = (JsObject)value;
            }
            else
            {
                var descriptor = GetDescriptor(index);
                if (
                    descriptor == null || (
                        descriptor.Owner != this &&
                        descriptor.DescriptorType == DescriptorType.Value
                    )
                )
                    DefineOwnProperty(new ValueDescriptor(this, index, value));
                else
                    descriptor.Set(this, value);
            }
        }

        public bool Delete(JsInstance index)
        {
            if (PropertyStore == null)
                return true;

            return PropertyStore.Delete(index);
        }

        public bool Delete(string index)
        {
            if (PropertyStore == null)
                return true;

            return PropertyStore.Delete(index);
        }

        public void DefineOwnProperty(string key, JsInstance value, PropertyAttributes attributes)
        {
            DefineOwnProperty(new ValueDescriptor(this, key, value, attributes));
        }

        public void DefineOwnProperty(string key, JsInstance value)
        {
            DefineOwnProperty(new ValueDescriptor(this, key, value));
        }

        public void DefineOwnProperty(Descriptor currentDescriptor)
        {
            EnsurePropertyStore();
            PropertyStore.DefineOwnProperty(currentDescriptor);
        }

        public void DefineAccessorProperty(string name, JsFunction getFunction, JsFunction setFunction)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            DefineOwnProperty(new PropertyDescriptor(Global, this, name, getFunction, setFunction, PropertyAttributes.None));
        }

        public IEnumerator<KeyValuePair<string, JsInstance>> GetEnumerator()
        {
            if (PropertyStore == null)
                return EmptyKeyValues.GetEnumerator();

            return PropertyStore.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<JsInstance> GetValues()
        {
            if (PropertyStore == null)
                return EmptyArray;

            return PropertyStore.GetValues();
        }

        public IEnumerable<string> GetKeys()
        {
            if (!IsPrototypeNull)
            {
                foreach (string key in Prototype.GetKeys())
                {
                    yield return key;
                }
            }

            if (PropertyStore != null)
            {
                foreach (string key in PropertyStore.GetKeys())
                {
                    yield return key;
                }
            }
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

        private bool IsPrototypeNull
        {
            get
            {
                Debug.Assert(Prototype != null);
                return Prototype == Global.PrototypeSink;
            }
        }
    }
}
