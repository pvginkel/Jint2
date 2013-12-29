﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Jint.Native
{
    partial class JintRuntime
    {
        public static bool Operation_Equal(object left, object right)
        {
            return CompareEquality(left, right);
        }

        public static bool Operation_Equal(object left, bool right)
        {
            return CompareEquality(left, right);
        }

        public static bool Operation_Equal(object left, double right)
        {
            return CompareEquality(left, right);
        }

        public static bool Operation_Equal(bool left, object right)
        {
            return CompareEquality(left, right);
        }

        public static bool Operation_Equal(bool left, double right)
        {
            return CompareEquality(left, right);
        }

        public static bool Operation_Equal(double left, object right)
        {
            return CompareEquality(left, right);
        }

        public static bool Operation_Equal(double left, bool right)
        {
            return CompareEquality(left, right);
        }

        public static bool Operation_NotEqual(object left, object right)
        {
            return !CompareEquality(left, right);
        }

        public static bool Operation_NotEqual(object left, bool right)
        {
            return !CompareEquality(left, right);
        }

        public static bool Operation_NotEqual(object left, double right)
        {
            return !CompareEquality(left, right);
        }

        public static bool Operation_NotEqual(bool left, object right)
        {
            return !CompareEquality(left, right);
        }

        public static bool Operation_NotEqual(bool left, double right)
        {
            return !CompareEquality(left, right);
        }

        public static bool Operation_NotEqual(double left, object right)
        {
            return !CompareEquality(left, right);
        }

        public static bool Operation_NotEqual(double left, bool right)
        {
            return !CompareEquality(left, right);
        }

        public static bool Operation_LessThan(object left, object right)
        {
            if (left is double && right is double)
                return (double)left < (double)right;

            double result;
            if (TryCompareRange(left, right, out result))
                return result < 0;

            return false;
        }

        public static bool Operation_LessThan(object left, bool right)
        {
            return JsValue.ToNumber(left) < JsConvert.ToNumber(right);
        }

        public static bool Operation_LessThan(object left, double right)
        {
            if (left is double)
                return (double)left < right;

            return JsValue.ToNumber(left) < right;
        }

        public static bool Operation_LessThan(bool left, object right)
        {
            return JsConvert.ToNumber(left) < JsValue.ToNumber(right);
        }

        public static bool Operation_LessThan(bool left, bool right)
        {
            return JsConvert.ToNumber(left) < JsConvert.ToNumber(right);
        }

        public static bool Operation_LessThan(bool left, double right)
        {
            return JsConvert.ToNumber(left) < right;
        }

        public static bool Operation_LessThan(double left, object right)
        {
            if (right is double)
                return left < (double)right;

            return left < JsValue.ToNumber(right);
        }

        public static bool Operation_LessThan(double left, bool right)
        {
            return left < JsConvert.ToNumber(right);
        }

        public static bool Operation_LessThanOrEqual(object left, object right)
        {
            if (left is double && right is double)
                return (double)left <= (double)right;

            double result;
            if (TryCompareRange(left, right, out result))
                return result <= 0;

            return false;
        }

        public static bool Operation_LessThanOrEqual(object left, bool right)
        {
            return JsValue.ToNumber(left) <= JsConvert.ToNumber(right);
        }

        public static bool Operation_LessThanOrEqual(object left, double right)
        {
            if (left is double)
                return (double)left <= right;

            return JsValue.ToNumber(left) <= right;
        }

        public static bool Operation_LessThanOrEqual(bool left, object right)
        {
            return JsConvert.ToNumber(left) <= JsValue.ToNumber(right);
        }

        public static bool Operation_LessThanOrEqual(bool left, bool right)
        {
            return JsConvert.ToNumber(left) <= JsConvert.ToNumber(right);
        }

        public static bool Operation_LessThanOrEqual(bool left, double right)
        {
            return JsConvert.ToNumber(left) <= right;
        }

        public static bool Operation_LessThanOrEqual(double left, object right)
        {
            if (right is double)
                return left <= (double)right;

            return left <= JsValue.ToNumber(right);
        }

        public static bool Operation_LessThanOrEqual(double left, bool right)
        {
            return left <= JsConvert.ToNumber(right);
        }

        public static bool Operation_GreaterThan(object left, object right)
        {
            if (left is double && right is double)
                return (double)left > (double)right;

            double result;
            if (TryCompareRange(left, right, out result))
                return result > 0;

            return false;
        }

        public static bool Operation_GreaterThan(object left, bool right)
        {
            return JsValue.ToNumber(left) > JsConvert.ToNumber(right);
        }

        public static bool Operation_GreaterThan(object left, double right)
        {
            if (left is double)
                return (double)left > right;

            return JsValue.ToNumber(left) > right;
        }

        public static bool Operation_GreaterThan(bool left, object right)
        {
            return JsConvert.ToNumber(left) > JsValue.ToNumber(right);
        }

        public static bool Operation_GreaterThan(bool left, bool right)
        {
            return JsConvert.ToNumber(left) > JsConvert.ToNumber(right);
        }

        public static bool Operation_GreaterThan(bool left, double right)
        {
            return JsConvert.ToNumber(left) > right;
        }

        public static bool Operation_GreaterThan(double left, object right)
        {
            if (right is double)
                return left > (double)right;

            return left > JsValue.ToNumber(right);
        }

        public static bool Operation_GreaterThan(double left, bool right)
        {
            return left > JsConvert.ToNumber(right);
        }

        public static bool Operation_GreaterThanOrEqual(object left, object right)
        {
            if (left is double && right is double)
                return (double)left >= (double)right;

            double result;
            if (TryCompareRange(left, right, out result))
                return result >= 0;

            return false;
        }

        public static bool Operation_GreaterThanOrEqual(object left, bool right)
        {
            return JsValue.ToNumber(left) >= JsConvert.ToNumber(right);
        }

        public static bool Operation_GreaterThanOrEqual(object left, double right)
        {
            if (left is double)
                return (double)left >= right;

            return JsValue.ToNumber(left) >= right;
        }

        public static bool Operation_GreaterThanOrEqual(bool left, object right)
        {
            return JsConvert.ToNumber(left) >= JsValue.ToNumber(right);
        }

        public static bool Operation_GreaterThanOrEqual(bool left, bool right)
        {
            return JsConvert.ToNumber(left) >= JsConvert.ToNumber(right);
        }

        public static bool Operation_GreaterThanOrEqual(bool left, double right)
        {
            return JsConvert.ToNumber(left) >= right;
        }

        public static bool Operation_GreaterThanOrEqual(double left, object right)
        {
            if (right is double)
                return left >= (double)right;

            return left >= JsValue.ToNumber(right);
        }

        public static bool Operation_GreaterThanOrEqual(double left, bool right)
        {
            return left >= JsConvert.ToNumber(right);
        }

        public static bool Operation_Same(object left, object right)
        {
            return CompareSame(left, right);
        }

        public static bool Operation_Same(object left, bool right)
        {
            return CompareSame(left, right);
        }

        public static bool Operation_Same(object left, double right)
        {
            return CompareSame(left, right);
        }

        public static bool Operation_Same(bool left, object right)
        {
            return CompareSame(left, right);
        }

        public static bool Operation_Same(bool left, double right)
        {
            return CompareSame(left, right);
        }

        public static bool Operation_Same(double left, object right)
        {
            return CompareSame(left, right);
        }

        public static bool Operation_Same(double left, bool right)
        {
            return CompareSame(left, right);
        }

        public static bool Operation_NotSame(object left, object right)
        {
            return !CompareSame(left, right);
        }

        public static bool Operation_NotSame(object left, bool right)
        {
            return !CompareSame(left, right);
        }

        public static bool Operation_NotSame(object left, double right)
        {
            return !CompareSame(left, right);
        }

        public static bool Operation_NotSame(bool left, object right)
        {
            return !CompareSame(left, right);
        }

        public static bool Operation_NotSame(bool left, double right)
        {
            return !CompareSame(left, right);
        }

        public static bool Operation_NotSame(double left, object right)
        {
            return !CompareSame(left, right);
        }

        public static bool Operation_NotSame(double left, bool right)
        {
            return !CompareSame(left, right);
        }

    }
}
