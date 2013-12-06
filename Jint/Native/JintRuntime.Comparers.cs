// ReSharper disable CompareOfFloatsByEqualityOperator

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JintRuntime
    {
        private static bool TryCompareRange(JsBox left, JsBox right, out double result)
        {
            result = 0;

            if (left.IsClr && right.IsClr)
            {
                var comparer = left.ToInstance().Value as IComparable;
                var rightInstance = right.ToInstance();

                if (
                    comparer == null ||
                    rightInstance.Value == null ||
                    comparer.GetType() != rightInstance.Value.GetType()
                )
                    return false;

                result = comparer.CompareTo(rightInstance.Value);
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

        public static bool CompareEquality(JsBox left, JsBox right)
        {
            if (left.IsClr && right.IsClr)
                return left.ToInstance().Value.Equals(right.ToInstance().Value);
            if (left.Type == right.Type)
            {
                // If both are Objects but then only one is CLR
                if (left.IsUndefined)
                    return true;
                if (left.IsNull)
                    return true;

                if (left.IsNumber)
                    return left.ToNumber() == right.ToNumber();
                if (left.IsString)
                    return left.ToString() == right.ToString();
                if (left.IsBoolean)
                    return left.ToBoolean() == right.ToBoolean();
                if (left.IsObject)
                    return (JsObject)left == (JsObject)right;
                return left.ToInstance().Value.Equals(right.ToInstance().Value);
            }
            if (left.IsNull && right.IsUndefined)
                return true;
            if (left.IsUndefined && right.IsNull)
                return true;
            if (left.IsNumber && right.IsString)
                return left.ToNumber() == right.ToNumber();
            if (left.IsString && right.IsNumber)
                return left.ToNumber() == right.ToNumber();
            if (left.IsBoolean || right.IsBoolean)
                return left.ToNumber() == right.ToNumber();
            if (right.IsObject && (left.IsString || left.IsNumber))
                return CompareEquality(left, right.ToPrimitive());
            if (left.IsObject && (right.IsString || right.IsNumber))
                return CompareEquality(left.ToPrimitive(), right);
            return false;
        }

        private static bool CompareEquality(JsBox left, bool right)
        {
            if (left.IsBoolean)
                return left.ToBoolean() == right;
            return left.ToNumber() == JsConvert.ToNumber(right);
        }

        private static bool CompareEquality(JsBox left, double right)
        {
            if (left.IsNumber)
                return left.ToNumber() == right;
            if (left.IsBoolean)
                return left.ToNumber() == right;
            if (left.IsObject)
                return CompareEquality(left.ToPrimitive(), right);
            return false;
        }

        private static bool CompareEquality(JsBox left, string right)
        {
            if (left.IsString)
                return left.ToString() == right;
            if (left.IsNumber)
                return left.ToNumber() == JsConvert.ToNumber(right);
            if (left.IsBoolean)
                return left.ToNumber() == JsConvert.ToNumber(right);
            if (left.IsObject)
                return CompareEquality(left.ToPrimitive(), right);
            return false;
        }

        private static bool CompareEquality(bool left, JsBox right)
        {
            if (right.IsBoolean)
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

        private static bool CompareEquality(double left, JsBox right)
        {
            if (right.IsNumber)
                return left == right.ToNumber();
            if (right.IsString)
                return left == right.ToNumber();
            if (right.IsObject)
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

        private static bool CompareEquality(string left, JsBox right)
        {
            if (right.IsString)
                return left == right.ToString();
            if (right.IsNumber)
                return JsConvert.ToNumber(left) == right.ToNumber();
            if (right.IsBoolean)
                return JsConvert.ToNumber(left) == right.ToNumber();
            if (right.IsObject)
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

        public static bool CompareSame(JsBox left, JsBox right)
        {
            if (left.Type != right.Type)
                return false;
            if (left.IsUndefined)
                return true;
            if (left.IsNull)
                return true;
            if (left.IsNumber)
                return left.ToNumber() == right.ToNumber();
            if (left.IsString)
                return left.ToString() == right.ToString();
            if (left.IsBoolean)
                return left.ToBoolean() == right.ToBoolean();
            return (JsObject)left == (JsObject)right;
        }

        private static bool CompareSame(JsBox left, bool right)
        {
            if (left.IsBoolean)
                return left.ToBoolean() == right;
            return false;
        }

        private static bool CompareSame(JsBox left, double right)
        {
            if (left.IsNumber)
                return left.ToNumber() == right;
            return false;
        }

        private static bool CompareSame(JsBox left, string right)
        {
            if (left.IsString)
                return left.ToString() == right;
            return false;
        }

        private static bool CompareSame(bool left, JsBox right)
        {
            if (right.IsBoolean)
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

        private static bool CompareSame(double left, JsBox right)
        {
            if (right.IsNumber)
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

        private static bool CompareSame(string left, JsBox right)
        {
            if (right.IsString)
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
