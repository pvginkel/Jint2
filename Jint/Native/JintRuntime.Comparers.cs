// ReSharper disable CompareOfFloatsByEqualityOperator

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JintRuntime
    {
        private static bool TryCompareRange(JsInstance left, JsInstance right, out double result)
        {
            result = 0;

            if (left.IsClr && right.IsClr)
            {
                var comparer = left.Value as IComparable;

                if (comparer == null || right.Value == null || comparer.GetType() != right.Value.GetType())
                    return false;

                result = comparer.CompareTo(right.Value);
            }
            else
            {
                double leftNumber = left.ToNumber();
                double rightNumber = right.ToNumber();

                if (Double.IsNaN(leftNumber) || Double.IsNaN(rightNumber))
                    return false;

                if (leftNumber < rightNumber)
                    result = -1;
                else if (leftNumber > rightNumber)
                    result = 1;
                else
                    result = 0;
            }

            return true;
        }

        public static bool CompareEquality(JsInstance left, JsInstance right)
        {
            if (left.IsClr && right.IsClr)
                return left.Value.Equals(right.Value);
            if (left.Type == right.Type)
            {
                // if both are Objects but then only one is Clrs
                if (JsInstance.IsUndefined(left))
                    return true;
                if (JsInstance.IsNull(left))
                    return true;

                if (left.Type == JsType.Number)
                    return left.ToNumber() == right.ToNumber();
                if (left.Type == JsType.String)
                    return left.ToString() == right.ToString();
                if (left.Type == JsType.Boolean)
                    return left.ToBoolean() == right.ToBoolean();
                if (left.Type == JsType.Object)
                    return left == right;
                return left.Value.Equals(right.Value);
            }
            if (JsInstance.IsNull(left) && JsInstance.IsUndefined(right))
                return true;
            if (JsInstance.IsUndefined(left) && JsInstance.IsNull(right))
                return true;
            if (left.Type == JsType.Number && right.Type == JsType.String)
                return left.ToNumber() == right.ToNumber();
            if (left.Type == JsType.String && right.Type == JsType.Number)
                return left.ToNumber() == right.ToNumber();
            if (left.Type == JsType.Boolean || right.Type == JsType.Boolean)
                return left.ToNumber() == right.ToNumber();
            if (right.Type == JsType.Object && (left.Type == JsType.String || left.Type == JsType.Number))
                return CompareEquality(left, right.ToPrimitive());
            if (left.Type == JsType.Object && (right.Type == JsType.String || right.Type == JsType.Number))
                return CompareEquality(left.ToPrimitive(), right);
            return false;
        }

        private static bool CompareEquality(JsInstance left, bool right)
        {
            if (left.Type == JsType.Boolean)
                return left.ToBoolean() == right;
            return left.ToNumber() == JsConvert.ToNumber(right);
        }

        private static bool CompareEquality(JsInstance left, double right)
        {
            if (left.Type == JsType.Number)
                return left.ToNumber() == right;
            if (left.Type == JsType.Boolean)
                return left.ToNumber() == right;
            if (left.Type == JsType.Object)
                return CompareEquality(left.ToPrimitive(), right);
            return false;
        }

        private static bool CompareEquality(JsInstance left, string right)
        {
            if (left.Type == JsType.String)
                return left.ToString() == right;
            if (left.Type == JsType.Number)
                return left.ToNumber() == JsConvert.ToNumber(right);
            if (left.Type == JsType.Boolean)
                return left.ToNumber() == JsConvert.ToNumber(right);
            if (left.Type == JsType.Object)
                return CompareEquality(left.ToPrimitive(), right);
            return false;
        }

        private static bool CompareEquality(bool left, JsInstance right)
        {
            if (right.Type == JsType.Boolean)
                return left == right.ToBoolean();
            return JsConvert.ToNumber(left) == right.ToNumber();
        }

        private static bool CompareEquality(bool left, double right)
        {
            return JsConvert.ToNumber(left) == right;
        }

        private static bool CompareEquality(bool left, string right)
        {
            return JsConvert.ToNumber(left) == JsConvert.ToNumber(right);
        }

        private static bool CompareEquality(double left, JsInstance right)
        {
            if (right.Type == JsType.Number)
                return left == right.ToNumber();
            if (right.Type == JsType.String)
                return left == right.ToNumber();
            if (right.Type == JsType.Object)
                return CompareEquality(left, right.ToPrimitive());
            return false;
        }

        private static bool CompareEquality(double left, bool right)
        {
            return left == JsConvert.ToNumber(right);
        }

        private static bool CompareEquality(double left, string right)
        {
            return left == JsConvert.ToNumber(right);
        }

        private static bool CompareEquality(string left, JsInstance right)
        {
            if (right.Type == JsType.String)
                return left == right.ToString();
            if (right.Type == JsType.Number)
                return JsConvert.ToNumber(left) == right.ToNumber();
            if (right.Type == JsType.Boolean)
                return JsConvert.ToNumber(left) == right.ToNumber();
            if (right.Type == JsType.Object)
                return CompareEquality(left, right.ToPrimitive());
            return false;
        }

        private static bool CompareEquality(string left, bool right)
        {
            return JsConvert.ToNumber(left) == JsConvert.ToNumber(right);
        }

        private static bool CompareEquality(string left, double right)
        {
            return JsConvert.ToNumber(left) == right;
        }

        public static bool CompareSame(JsInstance left, JsInstance right)
        {
            if (left.Type != right.Type)
                return false;
            if (JsInstance.IsUndefined(left))
                return true;
            if (left is JsNull)
                return true;
            if (left.Type == JsType.Number)
                return left.ToNumber() == right.ToNumber();
            if (left.Type == JsType.String)
                return left.ToString() == right.ToString();
            if (left.Type == JsType.Boolean)
                return left.ToBoolean() == right.ToBoolean();
            return left == right;
        }

        private static bool CompareSame(JsInstance left, bool right)
        {
            if (left.Type == JsType.Boolean)
                return left.ToBoolean() == right;
            return false;
        }

        private static bool CompareSame(JsInstance left, double right)
        {
            if (left.Type == JsType.Number)
                return left.ToNumber() == right;
            return false;
        }

        private static bool CompareSame(JsInstance left, string right)
        {
            if (left.Type == JsType.String)
                return left.ToString() == right;
            return false;
        }

        private static bool CompareSame(bool left, JsInstance right)
        {
            if (right.Type == JsType.Boolean)
                return left == right.ToBoolean();
            return false;
        }

        private static bool CompareSame(bool left, double right)
        {
            return false;
        }

        private static bool CompareSame(bool left, string right)
        {
            return false;
        }

        private static bool CompareSame(double left, JsInstance right)
        {
            if (right.Type == JsType.Number)
                return left == right.ToNumber();
            return false;
        }

        private static bool CompareSame(double left, bool right)
        {
            return false;
        }

        private static bool CompareSame(double left, string right)
        {
            return false;
        }

        private static bool CompareSame(string left, JsInstance right)
        {
            if (right.Type == JsType.String)
                return left == right.ToString();
            return false;
        }

        private static bool CompareSame(string left, bool right)
        {
            return false;
        }

        private static bool CompareSame(string left, double right)
        {
            return false;
        }
    }
}
