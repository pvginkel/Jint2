﻿using System;
using System.Collections.Generic;
using System.Text;
using Jint.Expressions;
using Jint.Runtime;

namespace Jint.Native
{
    /// <summary>
    /// A base class for values in javascript.
    /// </summary>
    [Serializable]
    public abstract class JsInstance : IComparable<JsInstance>
    {
        public static JsInstance[] EmptyArray = new JsInstance[0];

        public static bool IsNullOrUndefined(JsInstance instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            return instance.Type == JsType.Undefined || instance.Type == JsType.Null;
        }

        public static bool IsNull(JsInstance instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            return instance.Type == JsType.Null;
        }

        public static bool IsUndefined(JsInstance instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            return instance.Type == JsType.Undefined;
        }

        public virtual bool IsClr
        {
            get { return false; }
        }

        public abstract object Value { get; set; }

        public PropertyAttributes Attributes { get; set; }

        public JsInstance ToPrimitive()
        {
            return ToPrimitive(PreferredType.None);
        }

        public abstract JsInstance ToPrimitive(PreferredType preferredType);

        public abstract bool ToBoolean();

        public abstract double ToNumber();

        public virtual int ToInteger()
        {
            double result = ToNumber();

            if (Double.IsNaN(result))
                return 0;

            return (int)result;
        }

        public virtual object ToObject()
        {
            return Value;
        }

        public abstract override string ToString();

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : base.GetHashCode();
        }

        public virtual bool IsPrimitive
        {
            get { return false; }
        }

        /// <summary>
        /// Class of an object, don't confuse with type of an object.
        /// </summary>
        /// <remarks>There are only six object types in the ecma262.3: Undefined, Null, Boolean, String, Number, Object</remarks>
        public abstract string Class { get; }

        /// <summary>
        /// A type of a JsObject
        /// </summary>
        public abstract JsType Type { get; }

        // 11.9.6 The Strict Equality Comparison Algorithm
        public static bool StrictlyEquals(JsInstance left, JsInstance right)
        {
            return JintRuntime.CompareSame(left, right);
        }

        public int CompareTo(JsInstance other)
        {
            return ToString().CompareTo(other.ToString());
        }
    }
}
