using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using Jint.Expressions;
using Jint.PropertyBags;

namespace Jint.Native
{
    /// <summary>
    /// Base class for a JsObject class.
    /// </summary>
    /// <remarks>
    /// Implements generic property storing mechanism
    /// </remarks>
    [Serializable]
    [DebuggerTypeProxy(typeof(JsDictionaryObjectDebugView))]
    public abstract class JsDictionaryObject : JsInstance, IEnumerable<KeyValuePair<string, JsInstance>>
    {
        private readonly IPropertyBag _properties = new MiniCachedPropertyBag();

        /// <summary>
        /// Determines wheater object is extensible or not. Extensible object allows defining new own properties.
        /// </summary>
        /// <remarks>
        /// When object becomes non-extensible it can not become extansible again
        /// </remarks>
        public bool Extensible { get; set; }

        public bool HasChildren { get; private set; }

        private int _length;

        /// <summary>
        /// gets the number of an actually stored properties
        /// </summary>
        /// <remarks>
        /// This is a non ecma262 standart property
        /// </remarks>
        public virtual int Length { get { return _length; } set { } }

        /// <summary>
        /// Creates new Object without prototype
        /// </summary>
        public JsDictionaryObject()
        {
            Extensible = true;
            Prototype = JsNull.Instance;
        }

        /// <summary>
        /// Creates new object with an specified prototype
        /// </summary>
        /// <param name="prototype">Prototype</param>
        public JsDictionaryObject(JsDictionaryObject prototype)
        {
            Prototype = prototype;
            Extensible = true;
            prototype.HasChildren = true;
        }

        /// <summary>
        /// ecma262 [[prototype]] property
        /// </summary>
        private JsDictionaryObject Prototype { get; set; }

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
            JsDictionaryObject obj = this;
            while (true)
            {
                if (obj.HasOwnProperty(key))
                {
                    return true;
                }

                obj = obj.Prototype;

                if (obj is JsUndefined || obj == JsNull.Instance)
                {
                    return false;
                }
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

        public virtual Descriptor GetDescriptor(string index)
        {

            Descriptor result;
            if (_properties.TryGet(index, out result))
                if (!result.IsDeleted)
                    return result;
                else
                    _properties.Delete(index); // remove from cache

            // Prototype always a JsObject, (JsNull.Instance is also an object and next call will return null in case of null)
            if ((result = Prototype.GetDescriptor(index)) != null)
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
            get { return this[key.ToString()]; }
            set { this[key.ToString()] = value; }
        }

        public virtual JsInstance this[string index]
        {
            get
            {
                Descriptor d = GetDescriptor(index);
                return d != null ? d.Get(this) : JsUndefined.Instance;
            }
            set
            {
                Descriptor d = GetDescriptor(index);
                if (d == null || (d.Owner != this && d.DescriptorType == DescriptorType.Value))
                    DefineOwnProperty(new ValueDescriptor(this, index, value));
                else
                    d.Set(this, value);
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

            // TODO: In non-strict, this returns false.
            
            throw new JintException("Property " + index + " isn't configurable");
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

        public void DefineAccessorProperty(IGlobal global, string name, JsFunction get, JsFunction set)
        {
            if (global == null)
                throw new ArgumentNullException("global");
            if (name == null)
                throw new ArgumentNullException("name");

            DefineOwnProperty(new PropertyDescriptor(global, this, name)
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

        #region IEnumerable<KeyValuePair<JsInstance,JsInstance>> Members

        public IEnumerator<KeyValuePair<string, JsInstance>> GetEnumerator()
        {
            foreach (KeyValuePair<string, Descriptor> descriptor in _properties)
            {
                if (descriptor.Value.Enumerable)
                    yield return new KeyValuePair<string, JsInstance>(descriptor.Key, descriptor.Value.Get(this));
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _properties.GetEnumerator();
        }

        #endregion

        public virtual IEnumerable<JsInstance> GetValues()
        {
            foreach (Descriptor descriptor in _properties.Values)
            {
                if (descriptor.Enumerable)
                    yield return descriptor.Get(this);
            }
            yield break;
        }

        public virtual IEnumerable<string> GetKeys()
        {
            var p = Prototype;

            if (!(p is JsUndefined) && p != JsNull.Instance && p != null)
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
        public virtual JsInstance GetGetFunction(JsDictionaryObject target, JsInstance[] parameters)
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
        public virtual JsInstance GetSetFunction(JsDictionaryObject target, JsInstance[] parameters)
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

        [Obsolete("will be removed in 1.2", true)]
        public override object Call(IJintVisitor visitor, string function, params JsInstance[] parameters)
        {
            visitor.ExecuteFunction(this[function] as JsFunction, this, parameters);
            return visitor.Returned;
        }

        public override bool IsClr
        {
            get { return false; }
        }

        public bool IsPrototypeOf(JsDictionaryObject target)
        {
            if (target == null)
                return false;
            if (target is JsUndefined || target == JsNull.Instance)
                return false;
            if (target.Prototype == this)
                return true;
            return IsPrototypeOf(target.Prototype);
        }

        public override object Value
        {
            get { return null; }
            set { }
        }
    }
}
