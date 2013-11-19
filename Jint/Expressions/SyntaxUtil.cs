using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Native;

namespace Jint.Expressions
{
    internal static class SyntaxUtil
    {
        public static IList<T> ToReadOnly<T>(this IEnumerable<T> self)
        {
            if (self == null)
                return new T[0];

            return new ReadOnlyCollection<T>(self.ToList());
        }

        public static Type ToType(this ValueType self)
        {
            switch (self)
            {
                case ValueType.Boolean: return typeof(bool);
                case ValueType.Double: return typeof(double);
                case ValueType.String: return typeof(string);
                default: return typeof(JsInstance);
            }
        }

        public static Type GetTargetType(Type type)
        {
            return GetValueType(type).ToType();
        }

        public static ValueType GetValueType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Double: return ValueType.Double;
                case TypeCode.Boolean: return ValueType.Boolean;
                case TypeCode.String: return ValueType.String;
                case TypeCode.Object:
                    Debug.Assert(typeof(JsInstance).IsAssignableFrom(type));
                    return ValueType.Unknown;
                default: throw new NotImplementedException();
            }
        }
    }
}
