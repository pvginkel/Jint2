﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    internal enum DescriptorType
    {
        Value,
        Accessor,
        Clr
    }

    [Serializable]
    public abstract class Descriptor
    {
        protected Descriptor(JsObject owner, string name, PropertyAttributes attributes)
        {
            Attributes = attributes;
            Owner = owner;
            Name = name;
        }

        public string Name { get; set; }

        public PropertyAttributes Attributes { get; private set; }

        public bool Enumerable
        {
            get { return (Attributes & PropertyAttributes.DontEnum) == 0; }
        }

        public bool Configurable
        {
            get { return (Attributes & PropertyAttributes.DontDelete) == 0; }
        }

        public bool Writable
        {
            get { return (Attributes & PropertyAttributes.ReadOnly) == 0; }
        }

        public JsObject Owner { get; set; }

        public abstract bool IsReference { get; }

        public bool IsClr
        {
            get { return false; }
        }

        public abstract Descriptor Clone();

        /// <summary>
        /// Gets a value stored in the descriptor.
        /// </summary>
        /// <param name="that">A target object. This has a meaning in case of descriptors which helds an accessors,
        /// in value descriptors this parameter is ignored.</param>
        /// <returns>A value stored in the descriptor</returns>
        public abstract JsInstance Get(JsInstance that);

        /// <summary>
        /// Sets a value.
        /// </summary>
        /// <param name="that">A target object. This has a meaning in case of descriptors which helds an accessors,
        /// in value descriptors this parameter is ignored.</param>
        /// <param name="value">A new value which should be stored in the descriptor.</param>
        public abstract void Set(JsObject that, JsInstance value);

        internal abstract DescriptorType DescriptorType { get; }

        /// <summary>
        /// 8.10.5
        /// </summary>
        /// <param name="global"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static Descriptor ToPropertyDescriptor(JsGlobal global, JsObject owner, string name, JsInstance jsInstance)
        {
            if (jsInstance.Class != JsInstance.ClassObject)
            {
                throw new JsException(JsErrorType.TypeError, "The target object has to be an instance of an object");
            }

            JsObject obj = (JsObject)jsInstance;
            if ((obj.HasProperty("value") || obj.HasProperty("writable")) && (obj.HasProperty("set") || obj.HasProperty("get")))
                throw new JsException(JsErrorType.TypeError, "The property cannot be both writable and have get/set accessors or cannot have both a value and an accessor defined");

            var attributes = PropertyAttributes.None;
            JsFunction getFunction = null;
            JsFunction setFunction = null;
            JsInstance result;

            if (
                obj.TryGetProperty("enumerable", out result) &&
                !result.ToBoolean()
            )
                attributes |= PropertyAttributes.DontEnum;

            if (
                obj.TryGetProperty("configurable", out result) &&
                !result.ToBoolean()
            )
                attributes |= PropertyAttributes.DontDelete;

            if (
                obj.TryGetProperty("writable", out result) &&
                !result.ToBoolean()
            )
                attributes |= PropertyAttributes.ReadOnly;

            if (obj.TryGetProperty("get", out result))
            {
                if (!(result is JsFunction))
                    throw new JsException(JsErrorType.TypeError, "The getter has to be a function");

                getFunction = (JsFunction)result;
            }

            if (obj.TryGetProperty("set", out result))
            {
                if (!(result is JsFunction))
                    throw new JsException(JsErrorType.TypeError, "The setter has to be a function");

                setFunction = (JsFunction)result;
            }

            if (obj.HasProperty("value"))
                return new ValueDescriptor(owner, name, obj["value"]);
            else
                return new PropertyDescriptor(global, owner, name, getFunction, setFunction, attributes);
        }
    }
}
