using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            switch (value.GetJsType())
            {
                case JsType.String:
                case JsType.Number:
                case JsType.Boolean:
                case JsType.Undefined:
                case JsType.Null:
                    return true;

                default:
                    return false;
            }
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

        public static bool IsString(object value)
        {
            return value is string || value is JsString;
        }

        public static object UnwrapValue(object value)
        {
            var @object = value as JsObject;
            if (@object != null)
                return @object.Value;
            if (IsNullOrUndefined(value))
                return null;
            if (value is JsString)
                return value.ToString();

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
            var valueJsString = value as JsString;
            if (valueJsString != null)
                return JsConvert.ToNumber(valueJsString.ToString());
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
            if (IsNullOrUndefined(value))
                return false;
            var valueObject = value as JsObject;
            if (valueObject != null)
                return valueObject.ToBoolean();
            Debug.Assert(IsString(value));
            return JsConvert.ToBoolean(value.ToString());
        }

        public static string ToString(object value)
        {
            if (value is bool)
                return JsConvert.ToString((bool)value);
            if (value is double)
                return JsConvert.ToString((double)value);
            return value.ToString();
        }

        public static JsType GetJsType(this object value)
        {
            if (value is bool)
                return JsType.Boolean;
            if (value is double)
                return JsType.Number;
            if (value is string || value is JsString)
                return JsType.String;
            if (value is JsUndefined)
                return JsType.Undefined;
            if (value is JsNull)
                return JsType.Null;
            if (value is JsObject)
                return JsType.Object;
            throw new ArgumentOutOfRangeException("value", value.GetType().FullName);
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
            switch (GetJsType(value))
            {
                case JsType.Null:
                    return JsNames.TypeObject;

                case JsType.Object:
                    if (((JsObject)value).Delegate != null)
                        return JsNames.TypeFunction;
                    return JsNames.TypeObject;

                case JsType.Boolean:
                    return JsNames.TypeBoolean;

                case JsType.Number:
                    return JsNames.TypeNumber;

                case JsType.String:
                    return JsNames.TypeString;

                case JsType.Undefined:
                    return JsNames.TypeUndefined;

                default:
                    throw new ArgumentOutOfRangeException("value");
            }
        }

        public static string GetClass(object value)
        {
            switch (GetJsType(value))
            {
                case JsType.Null:
                case JsType.Undefined:
                    return JsNames.ClassObject;

                case JsType.Object:
                    return ((JsObject)value).Class;

                case JsType.String:
                    return JsNames.ClassString;

                case JsType.Boolean:
                    return JsNames.ClassBoolean;

                case JsType.Number:
                    return JsNames.ClassNumber;

                default:
                    throw new ArgumentOutOfRangeException("value");
            }
        }
    }
}
