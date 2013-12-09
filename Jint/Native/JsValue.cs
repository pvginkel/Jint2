using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    public static class JsValue
    {
        public static object[] EmptyArray = new object[0];

        public static bool IsFunction(object value)
        {
            var @object = value as JsObject;
            return @object != null && @object.Delegate != null;
        }

        public static bool IsPrimitive(object value)
        {
            var type = value.GetType();

            return
                type == typeof(string) ||
                type == typeof(double) ||
                type == typeof(bool) ||
                type == typeof(JsUndefined) ||
                type == typeof(JsNull);
        }

        public static bool IsClr(object value)
        {
            var instance = value as JsObject;
            if (instance != null)
                return instance.IsClr;
            return false;
        }

        public static bool IsNull(object value)
        {
            return value == JsNull.Instance;
        }

        public static bool IsUndefined(object value)
        {
            return value is JsUndefined;
        }

        public static bool IsNullOrUndefined(object value)
        {
            return IsNull(value) || IsUndefined(value);
        }

        public static object UnwrapValue(object value)
        {
            var @object = value as JsObject;
            if (@object != null)
                return @object.Value;
            if (IsNullOrUndefined(value))
                return null;

            return value;
        }

        public static double ToNumber(object value)
        {
            if (value is double)
                return (double)value;
            if (value is bool)
                return JsConvert.ToNumber((bool)value);
            string valueString = value as string;
            if (valueString != null)
                return JsConvert.ToNumber(valueString);
            if (IsNull(value))
                return 0d;
            if (IsUndefined(value))
                return Double.NaN;
            return ((JsObject)value).ToNumber();
        }

        public static bool ToBoolean(object value)
        {
            if (value is bool)
                return (bool)value;
            if (value is double)
                return JsConvert.ToBoolean((double)value);
            string valueString = value as string;
            if (valueString != null)
                return JsConvert.ToBoolean(valueString);
            if (IsNullOrUndefined(value))
                return false;
            return ((JsObject)value).ToBoolean();
        }

        public static string ToString(object value)
        {
            string valueString = value as string;
            if (valueString != null)
                return valueString;
            if (value is bool)
                return JsConvert.ToString((bool)value);
            if (value is double)
                return JsConvert.ToString((double)value);
            return value.ToString();
        }

        public static object ToPrimitive(object value)
        {
            var @object = value as JsObject;
            if (@object != null)
                return @object.ToPrimitive();

            return value;
        }

        public static string GetType(object value)
        {
            if (IsNull(value))
                return JsNames.TypeObject;
            var @object = value as JsObject;
            if (@object != null)
            {
                if (@object.Delegate != null)
                    return JsNames.TypeFunction;
                return JsNames.TypeObject;
            }
            if (value is bool)
                return JsNames.TypeBoolean;
            if (value is double)
                return JsNames.TypeNumber;
            if (value is string)
                return JsNames.TypeString;
            if (IsUndefined(value))
                return JsNames.TypeUndefined;
            throw new InvalidOperationException();
        }

        public static string GetClass(object value)
        {
            if (IsNullOrUndefined(value))
                return JsNames.ClassObject;
            var @object = value as JsObject;
            if (@object != null)
                return @object.Class;
            if (value is string)
                return JsNames.ClassString;
            if (value is bool)
                return JsNames.ClassBoolean;
            if (value is double)
                return JsNames.ClassNumber;
            throw new InvalidOperationException();
        }
    }
}
