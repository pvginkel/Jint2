using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Native;

namespace Jint.Runtime
{
    partial class JintRuntime
    {
        public JsInstance Operation_Add(JsInstance left, JsInstance right)
        {
            var leftPrimitive = left.ToPrimitive(Global);
            var rightPrimitive = right.ToPrimitive(Global);

            if (leftPrimitive is JsString || rightPrimitive is JsString)
                return _stringClass.New(String.Concat(leftPrimitive.ToString(), rightPrimitive.ToString()));

            return _numberClass.New(leftPrimitive.ToNumber() + rightPrimitive.ToNumber());
        }

        public JsInstance Operation_BitwiseAnd(JsInstance left, JsInstance right)
        {
            if (left is JsUndefined || right is JsUndefined)
                return _numberClass.New(0);

            return _numberClass.New(Convert.ToInt64(left.ToNumber()) & Convert.ToInt64(right.ToNumber()));
        }

        public JsInstance Operation_BitwiseExclusiveOr(JsInstance left, JsInstance right)
        {
            if (left is JsUndefined)
            {
                if (right is JsUndefined)
                    return _numberClass.New(1);
                return _numberClass.New(Convert.ToInt64(right.ToNumber()));
            }

            if (right is JsUndefined)
                return _numberClass.New(Convert.ToInt64(left.ToNumber()));

            return _numberClass.New(Convert.ToInt64(left.ToNumber()) ^ Convert.ToInt64(right.ToNumber()));
        }

        public JsInstance Operation_BitwiseNot(JsInstance operand)
        {
            return _numberClass.New(0 - operand.ToNumber() - 1);
        }

        public JsInstance Operation_BitwiseOr(JsInstance left, JsInstance right)
        {
            if (left is JsUndefined)
            {
                if (right is JsUndefined)
                    return _numberClass.New(1);

                return _numberClass.New(Convert.ToInt64(right.ToNumber()));
            }

            if (right is JsUndefined)
                return _numberClass.New(Convert.ToInt64(left.ToNumber()));

            return _numberClass.New(Convert.ToInt64(left.ToNumber()) | Convert.ToInt64(right.ToNumber()));
        }

        public JsInstance Operation_Divide(JsInstance left, JsInstance right)
        {
            var rightNumber = right.ToNumber();
            var leftNumber = left.ToNumber();

            if (right == _numberClass["NEGATIVE_INFINITY"] || right == _numberClass["POSITIVE_INFINITY"])
                return _numberClass.New(0);

            if (rightNumber == 0)
                return leftNumber > 0 ? _numberClass["POSITIVE_INFINITY"] : _numberClass["NEGATIVE_INFINITY"];

            return _numberClass.New(leftNumber / rightNumber);
        }

        public JsInstance Operation_Equal(JsInstance left, JsInstance right)
        {
            return _booleanClass.New(CompareEquality(left, right));
        }

        public JsInstance Operation_GreaterThan(JsInstance left, JsInstance right)
        {
            double result;
            if (TryCompareRange(left, right, out result))
                return _booleanClass.New(result > 0);

            return _booleanClass.False;
        }

        public JsInstance Operation_GreaterThanOrEqual(JsInstance left, JsInstance right)
        {
            double result;
            if (TryCompareRange(left, right, out result))
                return _booleanClass.New(result >= 0);

            return _booleanClass.False;
        }

        public JsInstance Operation_In(JsInstance left, JsInstance right)
        {
            if (right is ILiteral)
                throw new JsException(_errorClass.New("Cannot apply 'in' operator to the specified member."));

            return _booleanClass.New(((JsDictionaryObject)right).HasProperty(left));
        }

        public JsInstance Operation_InstanceOf(JsInstance left, JsInstance right)
        {
            var function = right as JsFunction;
            var obj = left as JsObject;

            if (function == null)
                throw new JsException(_typeErrorClass.New("Right argument should be a function"));
            if (obj == null)
                throw new JsException(_typeErrorClass.New("Left argument should be an object"));

            return _booleanClass.New(function.HasInstance(obj));
        }

        public JsInstance Operation_LeftShift(JsInstance left, JsInstance right)
        {
            if (left is JsUndefined)
                return _numberClass.New(0);
            if (right is JsUndefined)
                return _numberClass.New(Convert.ToInt64(left.ToNumber()));
            return _numberClass.New(Convert.ToInt64(left.ToNumber()) << Convert.ToUInt16(right.ToNumber()));
        }

        public JsInstance Operation_LessThan(JsInstance left, JsInstance right)
        {
            double result;
            if (TryCompareRange(left, right, out result))
                return _booleanClass.New(result < 0);

            return _booleanClass.False;
        }

        public JsInstance Operation_LessThanOrEqual(JsInstance left, JsInstance right)
        {
            double result;
            if (TryCompareRange(left, right, out result))
                return _booleanClass.New(result <= 0);

            return _booleanClass.False;
        }

        public JsInstance Operation_Modulo(JsInstance left, JsInstance right)
        {
            if (right == _numberClass["NEGATIVE_INFINITY"] || right == _numberClass["POSITIVE_INFINITY"])
                return _numberClass["POSITIVE_INFINITY"];
            if (right.ToNumber() == 0)
                return _numberClass["NaN"];
            return _numberClass.New(left.ToNumber() % right.ToNumber());
        }

        public JsInstance Operation_Multiply(JsInstance left, JsInstance right)
        {
            return _numberClass.New(
                left.ToNumber() * right.ToNumber()
            );
        }

        public JsInstance Operation_Negate(JsInstance operand)
        {
            return _numberClass.New(-operand.ToNumber());
        }

        public JsInstance Operation_Not(JsInstance operand)
        {
            return _booleanClass.New(!operand.ToBoolean());
        }

        public JsInstance Operation_NotEqual(JsInstance left, JsInstance right)
        {
            return _booleanClass.New(!CompareEquality(left, right));
        }

        public JsInstance Operation_NotSame(JsInstance left, JsInstance right)
        {
            var result = JsInstance.StrictlyEquals(Global, left, right);
            return _booleanClass.New(!result.ToBoolean());
        }

        public JsInstance Operation_Power(JsInstance left, JsInstance right)
        {
            return _numberClass.New(Math.Pow(left.ToNumber(), right.ToNumber()));
        }

        public JsInstance Operation_RightShift(JsInstance left, JsInstance right)
        {
            if (left is JsUndefined)
                return _numberClass.New(0);
            if (right is JsUndefined)
                return _numberClass.New(Convert.ToInt64(left.ToNumber()));
            return _numberClass.New(Convert.ToInt64(left.ToNumber()) >> Convert.ToUInt16(right.ToNumber()));
        }

        public JsInstance Operation_Same(JsInstance left, JsInstance right)
        {
            return JsInstance.StrictlyEquals(Global, left, right);
        }

        public JsInstance Operation_Subtract(JsInstance left, JsInstance right)
        {
            return _numberClass.New(
                left.ToNumber() - right.ToNumber()
            );
        }

        public JsInstance Operation_TypeOf(JsInstance operand)
        {
            if (operand == null)
                return _stringClass.New(JsUndefined.Instance.Type);
            if (operand is JsNull)
                return _stringClass.New(JsInstance.TypeObject);
            if (operand is JsFunction)
                return _stringClass.New(JsInstance.TypeofFunction);
            return _stringClass.New(operand.Type);
        }

        public JsInstance Operation_UnaryPlus(JsInstance operand)
        {
            return _numberClass.New(operand.ToNumber());
        }

        public JsInstance Operation_UnsignedRightShift(JsInstance left, JsInstance right)
        {
            if (left is JsUndefined)
                return _numberClass.New(0);
            if (right is JsUndefined)
                return _numberClass.New(Convert.ToInt64(left.ToNumber()));
            return _numberClass.New(Convert.ToInt64(left.ToNumber()) >> Convert.ToUInt16(right.ToNumber()));
        }

        private bool TryCompareRange(JsInstance left, JsInstance right, out double result)
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

        public bool CompareEquality(JsInstance left, JsInstance right)
        {
            if (left.IsClr && right.IsClr)
                return left.Value.Equals(right.Value);
            if (left.Type == right.Type)
            {
                // if both are Objects but then only one is Clrs
                if (left is JsUndefined)
                    return true;
                if (left == JsNull.Instance)
                    return true;

                if (left.Type == JsInstance.TypeNumber)
                {
                    if (left.ToNumber() == double.NaN)
                        return false;
                    if (right.ToNumber() == double.NaN)
                        return false;
                    if (left.ToNumber() == right.ToNumber())
                        return true;
                    return false;
                }
                if (left.Type == JsInstance.TypeString)
                    return left.ToString() == right.ToString();
                if (left.Type == JsInstance.TypeBoolean)
                    return left.ToBoolean() == right.ToBoolean();
                if (left.Type == JsInstance.TypeObject)
                    return left == right;
                return left.Value.Equals(right.Value);
            }
            if (left == JsNull.Instance && right is JsUndefined)
                return true;
            if (left is JsUndefined && right == JsNull.Instance)
                return true;
            if (left.Type == JsInstance.TypeNumber && right.Type == JsInstance.TypeString)
                return left.ToNumber() == right.ToNumber();
            if (left.Type == JsInstance.TypeString && right.Type == JsInstance.TypeNumber)
                return left.ToNumber() == right.ToNumber();
            if (left.Type == JsInstance.TypeBoolean || right.Type == JsInstance.TypeBoolean)
                return left.ToNumber() == right.ToNumber();
            if (right.Type == JsInstance.TypeObject && (left.Type == JsInstance.TypeString || left.Type == JsInstance.TypeNumber))
                return CompareEquality(left, right.ToPrimitive(Global));
            if (left.Type == JsInstance.TypeObject && (right.Type == JsInstance.TypeString || right.Type == JsInstance.TypeNumber))
                return CompareEquality(left.ToPrimitive(Global), right);
            return false;
        }

        public JsInstance Operation_Index(JsInstance obj, JsInstance index)
        {
            var stringObj = obj as JsString;
            var numberIndex = index as JsNumber;

            if (stringObj != null && numberIndex != null)
            {
                return _stringClass.New(
                    ((string)stringObj.Value).Substring((int)numberIndex.ToNumber(), 1)
                );
            }

            return ((JsDictionaryObject)obj)[index];
        }
    }
}
