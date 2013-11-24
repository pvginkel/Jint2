using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Jint.PropertyBags;

namespace Jint.Native
{
    [Serializable]
#if !DEBUG
    [DebuggerTypeProxy(typeof(JsObjectDebugView))]
#endif
    public class JsObject : JsInstance, IEnumerable<KeyValuePair<string, JsInstance>>
    {
        internal INativeIndexer Indexer { get; set; }

        private object _value;

        public override object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        private readonly MiniCachedPropertyBag _properties = new MiniCachedPropertyBag();

        /// <summary>
        /// Determines whether object is extensible or not. Extensible object allows defining new own properties.
        /// </summary>
        /// <remarks>
        /// When object becomes non-extensible it can not become extensible again
        /// </remarks>
        public bool Extensible { get; set; }

        private int _length;

        /// <summary>
        /// gets the number of an actually stored properties
        /// </summary>
        /// <remarks>
        /// This is a non ecma262 standard property
        /// </remarks>
        public virtual int Length
        {
            get { return _length; }
            set { }
        }

        public JsGlobal Global { get; private set; }

        /// <summary>
        /// ecma262 [[prototype]] property
        /// </summary>
        public JsObject Prototype { get; internal set; }

        public JsObject(JsGlobal global)
            : this(global, null, null)
        {
        }

        public JsObject(JsGlobal global, JsObject prototype)
            : this(global, null, prototype)
        {
        }

        public JsObject(JsGlobal global, object value, JsObject prototype)
        {
            if (global == null)
                throw new ArgumentNullException("global");

            Global = global;
            _value = value;
            Prototype = prototype ?? global.PrototypeSink;
            Extensible = true;
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

        public override int GetHashCode()
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
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

            throw new JsException(Global.TypeErrorClass.New("Invalid type"));
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
                EmptyArray,
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
        /// <param name="key">property name</param>
        /// <returns>true or false indicating check result</returns>
        /// <remarks>
        /// This implementation uses a HasOwnProperty method while walking a prototypes chain.
        /// </remarks>
        public virtual bool HasProperty(string key)
        {
            JsObject obj = this;
            while (true)
            {
                if (obj.HasOwnProperty(key))
                {
                    return true;
                }

                obj = obj.Prototype;

                if (IsNull(obj))
                    return false;
            }
        }

        /// <summary>
        /// Checks whether object has an own property
        /// </summary>
        /// <param name="key">property name</param>
        /// <returns>true of false</returns>
        public virtual bool HasOwnProperty(string key)
        {
            Descriptor desc;
            return _properties.TryGet(key, out desc) && desc.Owner == this;
        }

        public virtual bool HasProperty(JsInstance key)
        {
            return HasProperty(key.ToString());
        }

        public virtual bool HasOwnProperty(JsInstance key)
        {
            return HasOwnProperty(key.ToString());
        }

        public virtual Descriptor GetOwnDescriptor(string index)
        {
            Descriptor result;
            if (_properties.TryGet(index, out result))
            {
                if (!result.IsDeleted)
                    return result;

                _properties.Delete(index); // remove from cache
            }

            return null;
        }

        public virtual Descriptor GetDescriptor(string index)
        {
            var result = GetOwnDescriptor(index);
            if (result != null)
                return result;

            // Prototype always a JsObject, (JsNull.Instance is also an object and next call will return null in case of null)
            result = Prototype.GetDescriptor(index);
            if (result != null)
                _properties.Put(index, result); // cache descriptior

            return result;
        }

        public virtual bool TryGetDescriptor(JsInstance index, out Descriptor result)
        {
            return TryGetDescriptor(index.ToString(), out result);
        }

        public virtual bool TryGetDescriptor(string index, out Descriptor result)
        {
            result = GetDescriptor(index);
            return result != null;
        }

        public virtual bool TryGetProperty(JsInstance index, out JsInstance result)
        {
            return TryGetProperty(index.ToString(), out result);
        }

        public virtual bool TryGetProperty(string index, out JsInstance result)
        {
            Descriptor d = GetDescriptor(index);
            if (d == null)
            {
                result = JsUndefined.Instance;
                return false;
            }

            result = d.Get(this);

            return true;
        }

        public virtual JsInstance this[JsInstance key]
        {
            get
            {
                if (Indexer != null)
                    return Indexer.Get(this, key);

                return this[key.ToString()];
            }
            set
            {
                if (Indexer != null)
                    Indexer.Set(this, key, value);
                else
                    this[key.ToString()] = value;
            }
        }

        public virtual JsInstance this[string index]
        {
            get
            {
                Descriptor descriptor;

                if (index == "prototype")
                {
                    descriptor = GetOwnDescriptor("prototype");
                    if (descriptor != null)
                        return descriptor.Get(this);

                    return Prototype;
                }
                if (index == "__proto__")
                    return Prototype;

                descriptor = GetDescriptor(index);
                return
                    descriptor != null
                    ? descriptor.Get(this)
                    : JsUndefined.Instance;
            }
            set
            {
                if (index == "__proto__")
                {
                    var jsObject = value as JsObject;
                    if (jsObject != null)
                        Prototype = jsObject;
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
        }

        public virtual bool Delete(JsInstance key)
        {
            return Delete(key.ToString());
        }

        public virtual bool Delete(string index)
        {
            Descriptor d;
            if (!TryGetDescriptor(index, out d) || d.Owner != this)
                return true;

            if (d.Configurable)
            {
                _properties.Delete(index);
                d.Delete();
                _length--;
                return true;
            }

            return false;

            // TODO: This should throw in strict mode.
            
            // throw new JintException("Property " + index + " isn't configurable");
        }

        public void DefineOwnProperty(string key, JsInstance value, PropertyAttributes propertyAttributes)
        {
            DefineOwnProperty(new ValueDescriptor(this, key, value) { Writable = (propertyAttributes & PropertyAttributes.ReadOnly) == 0, Enumerable = (propertyAttributes & PropertyAttributes.DontEnum) == 0 });
        }

        public void DefineOwnProperty(string key, JsInstance value)
        {
            DefineOwnProperty(new ValueDescriptor(this, key, value));
        }

        public virtual void DefineOwnProperty(Descriptor currentDescriptor)
        {
            string key = currentDescriptor.Name;
            Descriptor desc;
            if (_properties.TryGet(key, out desc) && desc.Owner == this)
            {

                // updating an existing property
                switch (desc.DescriptorType)
                {
                    case DescriptorType.Value:
                        switch (currentDescriptor.DescriptorType)
                        {
                            case DescriptorType.Value:
                                _properties.Get(key).Set(this, currentDescriptor.Get(this));
                                break;

                            case DescriptorType.Accessor:
                                _properties.Delete(key);
                                _properties.Put(key, currentDescriptor);
                                break;

                            case DescriptorType.Clr:
                                throw new NotSupportedException();
                        }
                        break;

                    case DescriptorType.Accessor:
                        var propDesc = (PropertyDescriptor)desc;
                        if (currentDescriptor.DescriptorType == DescriptorType.Accessor)
                        {
                            propDesc.GetFunction = ((PropertyDescriptor)currentDescriptor).GetFunction ?? propDesc.GetFunction;
                            propDesc.SetFunction = ((PropertyDescriptor)currentDescriptor).SetFunction ?? propDesc.SetFunction;
                        }
                        else
                        {
                            propDesc.Set(this, currentDescriptor.Get(this));
                        }
                        break;
                }
            }
            else
            {
                // add a new property
                if (desc != null)
                    desc.Owner.RedefineProperty(desc.Name); // if we have a cached property

                _properties.Put(key, currentDescriptor);
                _length++;
            }
        }

        public void DefineAccessorProperty(string name, JsFunction get, JsFunction set)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            DefineOwnProperty(new PropertyDescriptor(Global, this, name)
            {
                GetFunction = get,
                SetFunction = set,
                Enumerable = true
            });
        }

        void RedefineProperty(string name)
        {
            Descriptor old;
            if (_properties.TryGet(name, out old) && old.Owner == this)
            {
                _properties.Put(name, old.Clone());
                old.Delete();
            }
        }

        public IEnumerator<KeyValuePair<string, JsInstance>> GetEnumerator()
        {
            foreach (KeyValuePair<string, Descriptor> descriptor in _properties)
            {
                if (descriptor.Value.Enumerable)
                    yield return new KeyValuePair<string, JsInstance>(descriptor.Key, descriptor.Value.Get(this));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _properties.GetEnumerator();
        }

        public virtual IEnumerable<JsInstance> GetValues()
        {
            foreach (Descriptor descriptor in _properties.Values)
            {
                if (descriptor.Enumerable)
                    yield return descriptor.Get(this);
            }
        }

        public virtual IEnumerable<string> GetKeys()
        {
            var p = Prototype;

            if (p != null && !IsNull(p))
            {
                foreach (string key in p.GetKeys())
                {
                    if (!HasOwnProperty(key))
                        yield return key;
                }
            }

            foreach (KeyValuePair<string, Descriptor> descriptor in _properties)
            {
                if (descriptor.Value.Enumerable && descriptor.Value.Owner == this)
                    yield return descriptor.Key;
            }
            yield break;
        }

        /// <summary>
        /// non standard
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="p"></param>
        /// <param name="currentDescriptor"></param>
        public static JsInstance GetGetFunction(JsObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
                throw new ArgumentException("propertyName");

            if (!target.HasOwnProperty(parameters[0].ToString()))
                return GetGetFunction(target.Prototype, parameters);

            var descriptor = target._properties.Get(parameters[0].ToString()) as PropertyDescriptor;
            if (descriptor == null)
                return JsUndefined.Instance;

            return (JsInstance)descriptor.GetFunction ?? JsUndefined.Instance;
        }

        /// <summary>
        /// non standard
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="p"></param>
        /// <param name="currentDescriptor"></param>
        public static JsInstance GetSetFunction(JsObject target, JsInstance[] parameters)
        {
            if (parameters.Length <= 0)
            {
                throw new ArgumentException("propertyName");
            }

            if (!target.HasOwnProperty(parameters[0].ToString()))
            {
                return GetSetFunction(target.Prototype, parameters);
            }

            PropertyDescriptor desc = target._properties.Get(parameters[0].ToString()) as PropertyDescriptor;
            if (desc == null)
            {
                return JsUndefined.Instance;
            }

            return (JsInstance)desc.SetFunction ?? JsUndefined.Instance;
        }

        public bool IsPrototypeOf(JsObject target)
        {
            if (target == null)
                return false;
            if (IsNull(target))
                return false;
            if (target.Prototype == this)
                return true;
            return IsPrototypeOf(target.Prototype);
        }
    }
}
