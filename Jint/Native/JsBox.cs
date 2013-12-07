using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    public struct JsBox
    {
        private readonly object _value;

        public static JsBox[] EmptyArray = new JsBox[0];
        public static readonly JsBox Undefined = new JsBox(JsUndefined.Instance);
        public static readonly JsBox Null = new JsBox(JsNull.Instance);
        public static readonly JsBox True = new JsBox(true);
        public static readonly JsBox False = new JsBox(false);
        public static readonly JsBox EmptyString = new JsBox(String.Empty);
        public static readonly JsBox MinValue = new JsBox(Double.MinValue);
        public static readonly JsBox MaxValue = new JsBox(Double.MaxValue);
        public static readonly JsBox NaN = new JsBox(Double.NaN);
        public static readonly JsBox NegativeInfinity = new JsBox(Double.NegativeInfinity);
        public static readonly JsBox PositiveInfinity = new JsBox(Double.PositiveInfinity);

        private JsBox(object value)
        {
            Debug.Assert(value == null || value.GetType() != typeof(JsBox));

            _value = value;
        }

        public Type Type
        {
            get { return _value.GetType(); }
        }

        public bool IsNumber
        {
            get { return _value.GetType() == typeof(double); }
        }

        public bool IsBoolean
        {
            get { return _value.GetType() == typeof(bool); }
        }

        public bool IsString
        {
            get { return _value.GetType() == typeof(string); }
        }

        public bool IsObject
        {
            get { return _value.GetType() == typeof(JsObject); }
        }

        public bool IsFunction
        {
            get
            {
                var @object = _value as JsObject;
                return @object != null && @object.Delegate != null;
            }
        }

        public bool IsPrimitive
        {
            get
            {
                var type = _value.GetType();

                return
                    type == typeof(string) ||
                    type == typeof(double) ||
                    type == typeof(bool) ||
                    type == typeof(JsUndefined) ||
                    type == typeof(JsNull);
            }
        }

        public bool IsLiteral
        {
            get { return !(_value is JsInstance); }
        }

        public bool IsClr
        {
            get
            {
                var instance = _value as JsInstance;
                if (instance != null)
                    return instance.IsClr;
                return false;
            }
        }

        public bool IsNull
        {
            get { return _value.GetType() == typeof(JsNull); }
        }

        public bool IsUndefined
        {
            get { return _value.GetType() == typeof(JsUndefined); }
        }

        public bool IsNullOrUndefined
        {
            get
            {
                var type = _value.GetType();
                return type == typeof(JsNull) || type == typeof(JsUndefined);
            }
        }

        public bool IsValid
        {
            get { return _value != null; }
        }

        public static JsBox CreateString(string value)
        {
            if (value == null)
                return EmptyString;
            return new JsBox(value);
        }

        public static JsBox CreateNumber(double value)
        {
            return new JsBox(value);
        }

        public static JsBox CreateBoolean(bool value)
        {
            return value ? True : False;
        }

        public static JsBox CreateObject(JsObject value)
        {
#if DEBUG
            if (value == null)
                throw new ArgumentNullException("value");
#endif

            return new JsBox(value);
        }

        public static JsBox FromInstance(JsInstance instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            if (
                instance is JsObject ||
                instance is JsNull ||
                instance is JsUndefined
            )
                return new JsBox(instance);

            Debug.Assert(
                instance.Value is string ||
                instance.Value is double ||
                instance.Value is bool
            );

            return new JsBox(instance.Value);
        }

        public static explicit operator JsObject(JsBox box)
        {
            return (JsObject)box._value;
        }

        public static explicit operator string(JsBox box)
        {
            return (string)box._value;
        }

        public static explicit operator bool(JsBox box)
        {
            return (bool)box._value;
        }

        public static explicit operator double(JsBox box)
        {
            return (double)box._value;
        }

        public JsInstance ToInstance()
        {
            var instance = _value as JsInstance;
            if (instance != null)
                return instance;

            if (IsNumber)
                return JsNumber.Create((double)_value);
            if (IsString)
                return JsString.Create((string)_value);
            if (IsBoolean)
                return JsBoolean.Create((bool)_value);

            throw new InvalidOperationException();
        }

        public double ToNumber()
        {
            if (IsNumber)
                return (double)_value;
            if (IsBoolean)
                return JsConvert.ToNumber((bool)_value);
            if (IsString)
                return JsConvert.ToNumber((string)_value);
            return ((JsInstance)_value).ToNumber();
        }

        public bool ToBoolean()
        {
            if (IsBoolean)
                return (bool)_value;
            if (IsNumber)
                return JsConvert.ToBoolean((double)_value);
            if (IsString)
                return JsConvert.ToBoolean((string)_value);
            return ((JsInstance)_value).ToBoolean();
        }

        public override string ToString()
        {
#if DEBUG
            if (_value == null)
                return null;
#endif

            if (IsString)
                return (string)_value;
            if (IsBoolean)
                return JsConvert.ToString((bool)_value);
            if (IsNumber)
                return JsConvert.ToString((double)_value);
            return _value.ToString();
        }

        public JsBox ToPrimitive()
        {
            var instance = _value as JsInstance;
            if (instance != null)
                return instance.ToPrimitive();

            return this;
        }

        public static JsBox CreateUndefined(string typeFullName)
        {
            if (typeFullName == null)
                throw new ArgumentNullException("typeFullName");

            return new JsBox(new JsUndefined(typeFullName));
        }

        public string GetTypeOf()
        {
            if (IsNull)
                return JsNames.TypeObject;
            var @object = _value as JsObject;
            if (@object != null)
            {
                if (@object.Delegate != null)
                    return JsNames.TypeFunction;
                return JsNames.TypeObject;
            }
            if (IsBoolean)
                return JsNames.TypeBoolean;
            if (IsNumber)
                return JsNames.TypeNumber;
            if (IsString)
                return JsNames.TypeString;
            if (IsUndefined)
                return JsNames.TypeUndefined;
            throw new InvalidOperationException();
        }

        public string GetClass()
        {
            var instance = _value as JsInstance;
            if (instance != null)
                return instance.Class;

            if (IsString)
                return JsNames.ClassString;
            if (IsBoolean)
                return JsNames.ClassBoolean;
            if (IsNumber)
                return JsNames.ClassNumber;
            throw new InvalidOperationException();
        }

        // 11.9.6 The Strict Equality Comparison Algorithm
        public static bool StrictlyEquals(JsBox left, JsBox right)
        {
            return JintRuntime.CompareSame(left, right);
        }

        // TODO: This is a temporary solution to give the Descriptor
        // access to the internal value.
        internal object GetValue()
        {
            return _value;
        }

        // TODO: This is a temporary solution to give the Descriptor
        // access to the internal value.
        internal static JsBox FromValue(object value)
        {
            return new JsBox(value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is JsBox))
                return false;

            return Equals(_value, ((JsBox)obj)._value);
        }

        public override int GetHashCode()
        {
            if (_value == null)
                return 0;
            return _value.GetHashCode();
        }
    }
}
