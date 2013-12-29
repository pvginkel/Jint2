using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Native;

namespace Jint.Bound
{
    internal static class BoundValueTypeExtensions
    {
        public static bool IsValueType(this BoundValueType self)
        {
            switch (self)
            {
                case BoundValueType.Boolean:
                case BoundValueType.Number:
                    return true;

                case BoundValueType.Unset:
                    throw new InvalidOperationException();

                default:
                    return false;
            }
        }

        public static Type GetNativeType(this BoundValueType self)
        {
            switch (self)
            {
                case BoundValueType.Boolean: return typeof(bool);
                case BoundValueType.Number: return typeof(double);
                case BoundValueType.Object: return typeof(JsObject);
                case BoundValueType.String: return typeof(string);
                case BoundValueType.Unknown: return typeof(object);
                default: throw new InvalidOperationException();
            }
        }

        public static BoundValueType ToValueType(this Type self)
        {
            switch (Type.GetTypeCode(self))
            {
                case TypeCode.Boolean: return BoundValueType.Boolean;
                case TypeCode.Double: return BoundValueType.Number;
                case TypeCode.String: return BoundValueType.String;
                default:
                    if (self == typeof(JsString))
                        return BoundValueType.String;
                    if (self == typeof(void))
                        return BoundValueType.Unset;
                    if (typeof(JsObject).IsAssignableFrom(self))
                        return BoundValueType.Object;
                    return BoundValueType.Unknown;
            }
        }
    }
}
