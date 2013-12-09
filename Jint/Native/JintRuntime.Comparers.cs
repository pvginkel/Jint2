// ReSharper disable CompareOfFloatsByEqualityOperator

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JintRuntime
    {
        private static bool TryCompareRange(object left, object right, out double result)
        {
            result = 0;

            if (JsValue.IsClr(left) && JsValue.IsClr(right))
            {
                var comparer = JsValue.UnwrapValue(left) as IComparable;
                var rightValue = JsValue.UnwrapValue(right);

                if (
                    comparer == null ||
                    rightValue == null ||
                    comparer.GetType() != rightValue.GetType()
                )
                    return false;

                result = comparer.CompareTo(rightValue);
            }
            else
            {
                double leftNumber = JsValue.ToNumber(left);
                double rightNumber = JsValue.ToNumber(right);

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

        public static bool CompareEquality(object left, object right)
        {
            if (JsValue.IsClr(left) && JsValue.IsClr(right))
                return JsValue.UnwrapValue(left).Equals(JsValue.UnwrapValue(right));
            if (left.GetType() == right.GetType())
            {
                // If both are Objects but then only one is CLR
                if (JsValue.IsUndefined(left))
                    return true;
                if (JsValue.IsNull(left))
                    return true;

                if (left is double)
                    return (double)left == (double)right;
                string leftString = left as string;
                if (leftString != null)
                    return leftString == (string)right;
                if (left is bool)
                    return (bool)left == (bool)right;
                if (left is JsObject)
                    return left == right;
                return JsValue.UnwrapValue(left).Equals(JsValue.UnwrapValue(right));
            }
            if (JsValue.IsNull(left) && JsValue.IsUndefined(right))
                return true;
            if (JsValue.IsUndefined(left) && JsValue.IsNull(right))
                return true;
            if (left is double && right is string)
                return (double)left == JsValue.ToNumber(right);
            if (left is string && right is double)
                return JsValue.ToNumber(left) == JsValue.ToNumber(right);
            if (left is bool || right is bool)
                return JsValue.ToNumber(left) == JsValue.ToNumber(right);
            if (right is JsObject && (left is string || left is double))
                return CompareEquality(left, JsValue.ToPrimitive(right));
            if (left is JsObject && (right is string || right is double))
                return CompareEquality(JsValue.ToPrimitive(left), right);
            return false;
        }

        private static bool CompareEquality(object left, bool right)
        {
            if (left is bool)
                return (bool)left == right;
            return JsValue.ToNumber(left) == JsConvert.ToNumber(right);
        }

        private static bool CompareEquality(object left, double right)
        {
            if (left is double)
                return (double)left == right;
            if (left is bool)
                return JsConvert.ToNumber((bool)left) == right;
            if (left is JsObject)
                return CompareEquality(JsValue.ToPrimitive(left), right);
            return false;
        }

        private static bool CompareEquality(object left, string right)
        {
            string leftString = left as string;
            if (leftString != null)
                return leftString == right;
            if (left is double)
                return (double)left == JsConvert.ToNumber(right);
            if (left is bool)
                return JsConvert.ToNumber((bool)left) == JsConvert.ToNumber(right);
            if (left is JsObject)
                return CompareEquality(JsValue.ToPrimitive(left), right);
            return false;
        }

        private static bool CompareEquality(bool left, object right)
        {
            if (right is bool)
                return left == (bool)right;
            return JsConvert.ToNumber(left) == JsValue.ToNumber(right);
        }

        private static bool CompareEquality(bool left, double right)
        {
            return JsConvert.ToNumber(left) == right;
        }

        private static bool CompareEquality(bool left, string right)
        {
            return JsConvert.ToNumber(left) == JsConvert.ToNumber(right);
        }

        private static bool CompareEquality(double left, object right)
        {
            if (right is double)
                return left == (double)right;
            if (right is string)
                return left == JsValue.ToNumber(right);
            if (right is JsObject)
                return CompareEquality(left, JsValue.ToPrimitive(right));
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

        private static bool CompareEquality(string left, object right)
        {
            string rightString = right as string;
            if (rightString != null)
                return left == rightString;
            if (right is double)
                return JsConvert.ToNumber(left) == (double)right;
            if (right is bool)
                return JsConvert.ToNumber(left) == JsConvert.ToNumber((bool)right);
            if (right is JsObject)
                return CompareEquality(left, JsValue.ToPrimitive(right));
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

        public static bool CompareSame(object left, object right)
        {
            if (left.GetType() != right.GetType())
                return false;
            if (JsValue.IsNullOrUndefined(left))
                return true;
            if (left is double)
                return (double)left == JsValue.ToNumber(right);
            string leftString = left as string;
            if (leftString != null)
                return leftString == JsValue.ToString(right);
            if (left is bool)
                return (bool)left == JsValue.ToBoolean(right);
            return left == right;
        }

        private static bool CompareSame(object left, bool right)
        {
            if (left is bool)
                return (bool)left == right;
            return false;
        }

        private static bool CompareSame(object left, double right)
        {
            if (left is double)
                return (double)left == right;
            return false;
        }

        private static bool CompareSame(object left, string right)
        {
            string leftString = left as string;
            if (leftString != null)
                return leftString == right;
            return false;
        }

        private static bool CompareSame(bool left, object right)
        {
            if (right is bool)
                return left == (bool)right;
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

        private static bool CompareSame(double left, object right)
        {
            if (right is double)
                return left == (double)right;
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

        private static bool CompareSame(string left, object right)
        {
            string rightString = right as string;
            if (rightString != null)
                return left == rightString;
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
