using System;
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
        public static JsInstance[] Empty = new JsInstance[0];

        public static bool IsNullOrUndefined(JsInstance o)
        {
            return (o is JsUndefined) || (o == JsNull.Instance) || (o.IsClr && o.Value == null);
        }

        public abstract bool IsClr { get; }

        public abstract object Value { get; set; }

        public PropertyAttributes Attributes { get; set; }

        public virtual JsInstance ToPrimitive(JsGlobal global)
        {
            return JsUndefined.Instance;
        }

        public virtual bool ToBoolean()
        {
            return true;
        }

        public virtual double ToNumber()
        {
            return 0;
        }

        public virtual int ToInteger()
        {
            return (int)ToNumber();
        }

        public virtual object ToObject()
        {
            return Value;
        }

        public virtual string ToSource()
        {
            return ToString();
        }

        public override string ToString()
        {
            return (Value ?? Class).ToString();
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : base.GetHashCode();
        }

        public virtual bool IsPrimitive
        {
            get { return false; }
        }

        public const string TypeObject = "object";
        public const string TypeBoolean = "boolean";
        public const string TypeString = "string";
        public const string TypeNumber = "number";
        public const string TypeUndefined = "undefined";
        public const string TypeNull = "null";

        public const string TypeDescriptor = "descriptor";

        public const string TypeFunction = "function"; // used only in typeof operator!!!

        // embed classes ecma262.3 15

        public const string ClassNumber = "Number";
        public const string ClassString = "String";
        public const string ClassBoolean = "Boolean";

        public const string ClassObject = "Object";
        public const string ClassFunction = "Function";
        public const string ClassArray = "Array";
        public const string ClassRegexp = "RegExp";
        public const string ClassDate = "Date";
        public const string ClassError = "Error";

        public const string ClassArguments = "Arguments";
        public const string ClassGlobal = "Global";
        public const string ClassDescriptor = "Descriptor";
        public const string ClassScope = "Scope";

        /// <summary>
        /// Class of an object, don't confuse with type of an object.
        /// </summary>
        /// <remarks>There are only six object types in the ecma262.3: Undefined, Null, Boolean, String, Number, Object</remarks>
        public abstract string Class { get; }

        /// <summary>
        /// A type of a JsObject
        /// </summary>
        public abstract JsType Type { get; }

        /// <summary>
        /// This is a shortcut to a function call by name.
        /// </summary>
        /// <remarks>
        /// Since this method requires a visitor it's not a very usefull, so this method is deprecated.
        /// </remarks>
        /// <param name="visitor"></param>
        /// <param name="function"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [Obsolete("will be removed in the 1.0 version", true)]
        public virtual object Call(IJintVisitor visitor, string function, params JsInstance[] parameters)
        {
            if (function == "toString")
                return visitor.Global.StringClass.New(ToString());
            return JsUndefined.Instance;
        }

        // 11.9.6 The Strict Equality Comparison Algorithm
        public static bool StrictlyEquals(JsInstance left, JsInstance right)
        {
            return JintRuntime.CompareSame(left, right);
        }

        #region IComparable<JsInstance> Members

        public int CompareTo(JsInstance other)
        {
            return ToString().CompareTo(other.ToString());
        }

        #endregion
    }
}
