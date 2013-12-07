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
    public sealed class JsObject : JsInstance, IEnumerable<KeyValuePair<string, JsBox>>
    {
        internal static readonly ReadOnlyCollection<KeyValuePair<int, JsBox>> EmptyKeyValues = new ReadOnlyCollection<KeyValuePair<int, JsBox>>(new KeyValuePair<int, JsBox>[0]);

        private object _value;
        private bool? _isClr;
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
            get
            {
                if (_isClr.HasValue)
                    return _isClr.Value;

                // If this instance holds a native value
                return _value != null;
            }
        }

        internal void SetIsClr(bool isClr)
        {
            _isClr = IsClr;
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

            var toString = GetDescriptor(Id.toString);
            var valueOf = GetDescriptor(Id.valueOf);

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

        private bool TryExecuteToPrimitiveFunction(Descriptor descriptor, out JsBox primitive)
        {
            var boxedThis = JsBox.CreateObject(this);

            var function = descriptor.Get(boxedThis);
            if (!function.IsFunction)
            {
                primitive = new JsBox();
                return false;
            }

            var result = Global.ExecuteFunction(
                (JsObject)function,
                boxedThis,
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

        /// <summary>
        /// Checks whether an object or it's [[prototype]] has the specified property.
        /// </summary>
        /// <param name="index">property name</param>
        /// <returns>true or false indicating check result</returns>
        /// <remarks>
        /// This implementation uses a HasOwnProperty method while walking a prototypes chain.
        /// </remarks>
        public bool HasProperty(string index)
        {
            return HasProperty(Global.ResolveIdentifier(index));
        }
        
        public bool HasProperty(int index)
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

        public bool HasProperty(JsBox index)
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

        public bool HasOwnProperty(int index)
        {
            if (PropertyStore == null)
                return false;

            return PropertyStore.HasOwnProperty(index);
        }

        public bool HasOwnProperty(JsBox index)
        {
            if (PropertyStore == null)
                return false;

            return PropertyStore.HasOwnProperty(index);
        }

        public Descriptor GetOwnDescriptor(int index)
        {
            if (PropertyStore == null)
                return null;

            return PropertyStore.GetOwnDescriptor(index);
        }

        public Descriptor GetOwnDescriptor(JsBox index)
        {
            if (PropertyStore == null)
                return null;

            return PropertyStore.GetOwnDescriptor(index);
        }

        public Descriptor GetDescriptor(int index)
        {
            var result = GetOwnDescriptor(index);
            if (result != null)
                return result;

            if (IsPrototypeNull)
                return null;

            return Prototype.GetDescriptor(index);
        }

        public Descriptor GetDescriptor(JsBox index)
        {
            var result = GetOwnDescriptor(index);
            if (result != null)
                return result;

            if (IsPrototypeNull)
                return null;

            return Prototype.GetDescriptor(index);
        }

        public bool TryGetDescriptor(JsBox index, out Descriptor result)
        {
            result = GetDescriptor(index);
            return result != null;
        }

        public bool TryGetDescriptor(int index, out Descriptor result)
        {
            result = GetDescriptor(index);
            return result != null;
        }

        public bool TryGetProperty(JsBox index, out JsBox result)
        {
            if (PropertyStore != null)
                return PropertyStore.TryGetProperty(index, out result);

            result = new JsBox();
            return false;
        }

        public bool TryGetProperty(int index, out JsBox result)
        {
            if (PropertyStore != null)
                return PropertyStore.TryGetProperty(index, out result);

            result = new JsBox();
            return false;
        }

        public JsBox this[JsBox index]
        {
            get { return GetProperty(index); }
            set { SetProperty(index, value); }
        }

        public JsBox this[string index]
        {
            get { return GetProperty(Global.ResolveIdentifier(index)); }
            set { SetProperty(Global.ResolveIdentifier(index), value); }
        }

        private JsBox GetProperty(JsBox index)
        {
            if (PropertyStore != null)
            {
                JsBox result;
                if (PropertyStore.TryGetProperty(index, out result))
                    return result;
            }

            return GetPropertyCore(Global.ResolveIdentifier(index.ToString()));
        }

        public JsBox GetProperty(int index)
        {
            if (PropertyStore != null)
            {
                JsBox result;
                if (PropertyStore.TryGetProperty(index, out result))
                    return result;
            }

            return GetPropertyCore(index);
        }

        private JsBox GetPropertyCore(int index)
        {
            Descriptor descriptor;

            if (index == Id.prototype)
            {
                descriptor = GetOwnDescriptor(index);
                if (descriptor != null)
                    return descriptor.Get(JsBox.CreateObject(this));

                if (IsPrototypeNull)
                    return JsBox.Null;

                return JsBox.CreateObject(Prototype);
            }

            if (index == Id.__proto__)
            {
                if (IsPrototypeNull)
                    return JsBox.Null;

                return JsBox.CreateObject(Prototype);
            }

            descriptor = GetDescriptor(index);

            if (descriptor == null)
            {
                Trace.WriteLine(String.Format(
                    "Unresolved identifier {0} of {1}",
                    Global.GetIdentifier(index),
                    GetType()
                ));
            }

            return
                descriptor != null
                ? descriptor.Get(JsBox.CreateObject(this))
                : JsBox.Undefined;
        }

        public JsBox SetProperty(int index, JsBox value)
        {
            EnsurePropertyStore();
            if (!PropertyStore.TrySetProperty(index, value))
                SetPropertyCore(index, value);

            return value;
        }

        private void SetProperty(JsBox index, JsBox value)
        {
            EnsurePropertyStore();
            if (!PropertyStore.TrySetProperty(index, value))
                SetPropertyCore(Global.ResolveIdentifier(index.ToString()), value);
        }

        private void SetPropertyCore(int index, JsBox value)
        {
            if (index == Id.__proto__)
            {
                if (value.IsObject)
                    Prototype = (JsObject)value;
            }
            else
            {
                // If we have this descriptor, we can set it using that descriptor.

                var descriptor = GetOwnDescriptor(index);
                if (descriptor != null)
                {
                    descriptor.Set(JsBox.CreateObject(this), value);
                    return;
                }

                // If we don't have it, the prototype may have it.
                
                if (!IsPrototypeNull)
                {
                    descriptor = Prototype.GetDescriptor(index);

                    // If the prototype has it and it's an accessor; go through
                    // that accessor.

                    if (descriptor != null && descriptor.IsAccessor)
                    {
                        descriptor.Set(JsBox.CreateObject(this), value);
                        return;
                    }
                }

                // We don't have the property yet and there is no accessor
                // defined; create a new property.

                DefineOwnProperty(new Descriptor(
                    index,
                    value,
                    PropertyAttributes.None
                ));
            }
        }

        public bool Delete(JsBox index)
        {
            if (PropertyStore == null)
                return true;

            return PropertyStore.Delete(index);
        }

        public bool Delete(string index)
        {
            return Delete(Global.ResolveIdentifier(index));
        }

        public bool Delete(int index)
        {
            if (PropertyStore == null)
                return true;

            return PropertyStore.Delete(index);
        }

        public void DefineOwnProperty(string key, JsBox value)
        {
            DefineOwnProperty(key, value, PropertyAttributes.None);
        }

        public void DefineOwnProperty(string key, JsBox value, PropertyAttributes attributes)
        {
            DefineOwnProperty(new Descriptor(Global.ResolveIdentifier(key), value, attributes));
        }

        public void DefineOwnProperty(Descriptor currentDescriptor)
        {
            EnsurePropertyStore();
            PropertyStore.DefineOwnProperty(currentDescriptor);
        }

        public void DefineAccessorProperty(int index, JsObject getFunction, JsObject setFunction)
        {
            DefineOwnProperty(new Descriptor(
                index,
                getFunction,
                setFunction,
                PropertyAttributes.None
            ));
        }

        public IEnumerator<KeyValuePair<string, JsBox>> GetEnumerator()
        {
            var items =
                PropertyStore == null
                ? EmptyKeyValues.GetEnumerator()
                : PropertyStore.GetEnumerator();

            using (items)
            {
                while (items.MoveNext())
                {
                    yield return new KeyValuePair<string, JsBox>(
                        Global.GetIdentifier(items.Current.Key),
                        items.Current.Value
                    );
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<JsBox> GetValues()
        {
            if (PropertyStore == null)
                return JsBox.EmptyArray;

            return PropertyStore.GetValues();
        }

        public IEnumerable<int> GetKeys()
        {
            if (!IsPrototypeNull)
            {
                foreach (int key in Prototype.GetKeys())
                {
                    yield return key;
                }
            }

            if (PropertyStore != null)
            {
                foreach (int key in PropertyStore.GetKeys())
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
