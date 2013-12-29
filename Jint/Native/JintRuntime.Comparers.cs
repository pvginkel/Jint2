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

            var leftType = left.GetJsType();
            var rightType = right.GetJsType();

            if (leftType == rightType)
            {
                // If both are Objects but then only one is CLR
                switch (leftType)
                {
                    case JsType.Undefined:
                    case JsType.Null: return true;
                    case JsType.Number: return (double)left == (double)right;
                    case JsType.String: return left.ToString() == right.ToString();
                    case JsType.Boolean: return (bool)left == (bool)right;
                    case JsType.Object: return left == right;
                    default: return JsValue.UnwrapValue(left).Equals(JsValue.UnwrapValue(right));
                }
            }

            if (leftType == JsType.Null && rightType == JsType.Undefined)
                return true;
            if (leftType == JsType.Undefined && rightType == JsType.Null)
                return true;
            if (leftType == JsType.Number && rightType == JsType.String)
                return (double)left == JsValue.ToNumber(right);
            if (leftType == JsType.String && rightType == JsType.Null)
                return JsValue.ToNumber(left) == JsValue.ToNumber(right);
            if (leftType == JsType.Boolean || rightType == JsType.Boolean)
                return JsValue.ToNumber(left) == JsValue.ToNumber(right);
            if (rightType == JsType.Object && (leftType == JsType.String || leftType == JsType.Number))
                return CompareEquality(left, JsValue.ToPrimitive(right));
            if (leftType == JsType.Object && (rightType == JsType.String || rightType == JsType.Number))
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

        private static bool CompareEquality(double left, object right)
        {
            switch (right.GetJsType())
            {
                case JsType.Number: return left == (double)right;
                case JsType.String: return left == JsValue.ToNumber(right);
                case JsType.Object: return CompareEquality(left, JsValue.ToPrimitive(right));
                default: return false;
            }
        }

        private static bool CompareEquality(double left, bool right)
        {
            return left == JsConvert.ToNumber(right);
        }

        public static bool CompareSame(object left, object right)
        {
            var leftType = left.GetJsType();
            var rightType = right.GetJsType();

            if (leftType != rightType)
                return false;

            switch (leftType)
            {
                case JsType.Null:
                case JsType.Undefined: return true;
                case JsType.Number: return (double)left == JsValue.ToNumber(right);
                case JsType.String: return left.ToString() == JsValue.ToString(right);
                case JsType.Boolean: return (bool)left == JsValue.ToBoolean(right);
                default: return left == right;
            }
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
    }
}
