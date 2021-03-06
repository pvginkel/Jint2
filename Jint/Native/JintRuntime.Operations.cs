﻿// ReSharper disable CompareOfFloatsByEqualityOperator

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JintRuntime
    {
        public static object Operation_Add(object left, object right)
        {
            if (left is double && right is double)
                return (double)left + (double)right;

            var leftPrimitive = JsValue.ToPrimitive(left);
            var rightPrimitive = JsValue.ToPrimitive(right);

            var leftJsString = leftPrimitive as JsString;
            if (leftJsString != null)
                return JsString.Concat(leftJsString, rightPrimitive);
            var rightJsString = rightPrimitive as JsString;
            if (rightJsString != null)
                return JsString.Concat(leftPrimitive, rightJsString);

            var leftString = leftPrimitive as string;
            if (leftString != null)
                return JsString.Concat(leftString, rightPrimitive);
            var rightString = rightPrimitive as string;
            if (rightString != null)
                return JsString.Concat(leftPrimitive, rightString);

            return JsValue.ToNumber(leftPrimitive) + JsValue.ToNumber(rightPrimitive);
        }

        public static object Operation_Add(double left, object right)
        {
            if (right is double)
                return left + (double)right;

            var rightPrimitive = JsValue.ToPrimitive(right);

            var rightJsString = rightPrimitive as JsString;
            if (rightJsString != null)
                return JsString.Concat(left.ToString(CultureInfo.InvariantCulture), rightJsString);

            string rightString = rightPrimitive as string;
            if (rightString != null)
                return JsString.Concat(left.ToString(CultureInfo.InvariantCulture), rightString);

            return left + JsValue.ToNumber(rightPrimitive);
        }

        public static object Operation_Add(object left, double right)
        {
            if (left is double)
                return (double)left + right;

            var leftPrimitive = JsValue.ToPrimitive(left);

            var leftJsString = leftPrimitive as JsString;
            if (leftJsString != null)
                return JsString.Concat(leftJsString, right.ToString(CultureInfo.InvariantCulture));

            string leftString = leftPrimitive as string;
            if (leftString != null)
                return JsString.Concat(leftString, right.ToString(CultureInfo.InvariantCulture));

            return JsValue.ToNumber(leftPrimitive) + right;
        }

        public static double Operation_BitwiseAnd(object left, object right)
        {
            if (left is double && right is double)
                return (long)(double)left & (long)(double)right;

            if (JsValue.IsUndefined(left) || JsValue.IsUndefined(right))
                return 0;

            return (long)JsValue.ToNumber(left) & (long)JsValue.ToNumber(right);
        }

        public static double Operation_BitwiseAnd(double left, object right)
        {
            if (JsValue.IsUndefined(right))
                return 0;

            return (long)left & (long)JsValue.ToNumber(right);
        }

        public static double Operation_BitwiseAnd(object left, double right)
        {
            if (left is double)
                return (long)(double)left & (long)right;

            if (JsValue.IsUndefined(left))
                return 0;

            return (long)JsValue.ToNumber(left) & (long)right;
        }

        public static double Operation_BitwiseExclusiveOr(object left, object right)
        {
            if (left is double && right is double)
                return (long)(double)left ^ (long)(double)right;

            if (JsValue.IsUndefined(left))
            {
                if (JsValue.IsUndefined(right))
                    return 1;

                return (long)JsValue.ToNumber(right);
            }

            if (JsValue.IsUndefined(right))
                return (long)JsValue.ToNumber(left);

            return (long)JsValue.ToNumber(left) ^ (long)JsValue.ToNumber(right);
        }

        public static double Operation_BitwiseExclusiveOr(double left, object right)
        {
            if (right is double)
                return (long)left ^ (long)(double)right;

            if (JsValue.IsUndefined(right))
                return (long)left;

            return (long)left ^ (long)JsValue.ToNumber(right);
        }

        public static double Operation_BitwiseExclusiveOr(object left, double right)
        {
            if (left is double)
                return (long)(double)left ^ (long)right;

            if (JsValue.IsUndefined(left))
                return (long)right;

            return (long)JsValue.ToNumber(left) ^ (long)right;
        }

        public static double Operation_BitwiseNot(object operand)
        {
            if (operand is double)
                return Operation_BitwiseNot((double)operand);

            var number = JsValue.ToNumber(JsValue.ToPrimitive(operand));

            if (Double.IsNaN(number) || Double.IsInfinity(number))
                number = 0;

            return -((long)number + 1);
        }

        public static double Operation_BitwiseNot(double operand)
        {
            if (Double.IsNaN(operand) || Double.IsInfinity(operand))
                operand = 0;

            return -((long)operand + 1);
        }

        public static double Operation_BitwiseOr(object left, object right)
        {
            if (left is double && right is double)
                return (long)(double)left | (long)(double)right;

            if (JsValue.IsUndefined(left))
            {
                if (JsValue.IsUndefined(right))
                    return 1;

                return (long)JsValue.ToNumber(right);
            }

            if (JsValue.IsUndefined(right))
                return (long)JsValue.ToNumber(left);

            return (long)JsValue.ToNumber(left) | (long)JsValue.ToNumber(right);
        }

        public static double Operation_BitwiseOr(double left, object right)
        {
            if (right is double)
                return (long)left | (long)(double)right;

            if (JsValue.IsUndefined(right))
                return (long)left;

            return (long)left | (long)JsValue.ToNumber(right);
        }

        public static double Operation_BitwiseOr(object left, double right)
        {
            if (left is double)
                return (long)(double)left | (long)right;

            if (JsValue.IsUndefined(left))
                return (long)right;

            return (long)JsValue.ToNumber(left) | (long)right;
        }

        public static double Operation_Divide(object left, object right)
        {
            if (left is double && right is double)
                return Operation_Divide((double)left, (double)right);

            return Operation_Divide(JsValue.ToNumber(left), JsValue.ToNumber(right));
        }

        public static double Operation_Divide(double left, object right)
        {
            if (right is double)
                return Operation_Divide(left, (double)right);

            return Operation_Divide(left, JsValue.ToNumber(right));
        }

        public static double Operation_Divide(object left, double right)
        {
            if (left is double)
                return Operation_Divide((double)left, right);

            return Operation_Divide(JsValue.ToNumber(left), right);
        }

        public static double Operation_Divide(double left, double right)
        {
            if (Double.IsInfinity(right))
                return 0;

            if (right == 0)
                return left > 0 ? Double.PositiveInfinity : Double.NegativeInfinity;

            return left / right;
        }

        public bool Operation_In(object left, object right)
        {
            var @object = right as JsObject;
            if (@object == null)
                throw new JsException(JsErrorType.Error, "Cannot apply 'in' operator to the specified member.");

            return @object.HasProperty(left);
        }

        public bool Operation_InstanceOf(object left, object right)
        {
            var rightObject = right as JsObject;
            if (rightObject == null || rightObject.Delegate == null)
                throw new JsException(JsErrorType.TypeError, "Right argument should be a function");
            var leftObject = left as JsObject;
            if (leftObject == null)
                return false;

            return rightObject.HasInstance(leftObject);
        }

        public static double Operation_LeftShift(object left, object right)
        {
            if (left is double && right is double)
                return (long)(double)left << (ushort)(double)right;

            return OperationSow_LeftShift(left, right);
        }

        private static double OperationSow_LeftShift(object left, object right)
        {
            if (JsValue.IsUndefined(left))
                return 0;
            if (JsValue.IsUndefined(right))
                return (long)JsValue.ToNumber(left);
            return (long)JsValue.ToNumber(left) << (ushort)JsValue.ToNumber(right);
        }

        public static double Operation_LeftShift(double left, object right)
        {
            if (right is double)
                return (long)left << (ushort)(double)right;

            if (JsValue.IsUndefined(right))
                return (long)left;
            return (long)left << (ushort)JsValue.ToNumber(right);
        }

        public static double Operation_LeftShift(object left, double right)
        {
            if (left is double)
                return (long)(double)left << (ushort)right;

            if (JsValue.IsUndefined(left))
                return 0;
            return (long)JsValue.ToNumber(left) << (ushort)right;
        }

        public static double Operation_Modulo(object left, object right)
        {
            if (left is double && right is double)
                return Operation_Modulo((double)left, (double)right);

            double rightNumber = JsValue.ToNumber(right);
            if (Double.IsInfinity(rightNumber))
                return Double.PositiveInfinity;
            if (rightNumber == 0)
                return Double.NaN;
            return JsValue.ToNumber(left) % rightNumber;
        }

        public static double Operation_Modulo(double left, object right)
        {
            if (right is double)
                return Operation_Modulo(left, (double)right);

            double rightNumber = JsValue.ToNumber(right);
            if (Double.IsInfinity(rightNumber))
                return Double.PositiveInfinity;
            return left % rightNumber;
        }

        public static double Operation_Modulo(object left, double right)
        {
            if (left is double)
                return Operation_Modulo((double)left, right);

            if (Double.IsInfinity(right))
                return Double.PositiveInfinity;
            if (right == 0)
                return Double.NaN;
            return JsValue.ToNumber(left) % right;
        }

        public static double Operation_Modulo(double left, double right)
        {
            if (Double.IsInfinity(right))
                return Double.PositiveInfinity;
            if (right == 0)
                return Double.NaN;
            return left % right;
        }

        public static double Operation_Multiply(object left, object right)
        {
            if (left is double && right is double)
                return (double)left * (double)right;

            return JsValue.ToNumber(left) * JsValue.ToNumber(right);
        }

        public static double Operation_Multiply(double left, object right)
        {
            if (right is double)
                return left * (double)right;

            return left * JsValue.ToNumber(right);
        }

        public static double Operation_Multiply(object left, double right)
        {
            if (left is double)
                return (double)left * right;

            return JsValue.ToNumber(left) * right;
        }

        public static double Operation_Negate(object operand)
        {
            if (operand is double)
                return -(double)operand;

            return -JsValue.ToNumber(operand);
        }

        public static bool Operation_Not(object operand)
        {
            if (operand is bool)
                return !(bool)operand;

            return !JsValue.ToBoolean(operand);
        }

        public static double Operation_RightShift(object left, object right)
        {
            if (left is double && right is double)
                return (long)(double)left >> (ushort)(double)right;

            if (JsValue.IsUndefined(left))
                return 0;
            if (JsValue.IsUndefined(right))
                return (long)JsValue.ToNumber(left);
            return (long)JsValue.ToNumber(left) >> (ushort)JsValue.ToNumber(right);
        }

        public static double Operation_RightShift(double left, object right)
        {
            if (right is double)
                return (long)left >> (ushort)(double)right;

            if (JsValue.IsUndefined(right))
                return (long)left;
            return (long)left >> (ushort)JsValue.ToNumber(right);
        }

        public static double Operation_RightShift(object left, double right)
        {
            if (left is double)
                return (long)(double)left >> (ushort)right;

            if (JsValue.IsUndefined(left))
                return 0;
            return (long)JsValue.ToNumber(left) >> (ushort)right;
        }

        public static double Operation_Subtract(object left, object right)
        {
            if (left is double && right is double)
                return (double)left - (double)right;

            return JsValue.ToNumber(left) - JsValue.ToNumber(right);
        }

        public static double Operation_Subtract(double left, object right)
        {
            if (right is double)
                return left - (double)right;

            return left - JsValue.ToNumber(right);
        }

        public static double Operation_Subtract(object left, double right)
        {
            if (left is double)
                return (double)left - right;

            return JsValue.ToNumber(left) - right;
        }

        public static string Operation_TypeOf(object operand)
        {
            return JsValue.GetType(operand);
        }

        public static string Operation_TypeOf(JsObject scope, string identifier)
        {
            object value = scope.GetPropertyRaw(scope.Global.ResolveIdentifier(identifier));
            if (value == null)
                return JsNames.TypeUndefined;

            var accessor = value as PropertyAccessor;
            if (accessor != null)
                return Operation_TypeOf(accessor.GetValue(scope));

            return Operation_TypeOf(value);
        }

        public static double Operation_UnaryPlus(object operand)
        {
            if (operand is double)
                return (double)operand;

            return JsValue.ToNumber(operand);
        }

        public static double Operation_UnsignedRightShift(object left, object right)
        {
            if (left is double && right is double)
                return (long)(double)left >> (ushort)(double)right;

            if (JsValue.IsUndefined(left))
                return 0;
            if (JsValue.IsUndefined(right))
                return (long)JsValue.ToNumber(left);
            return (long)JsValue.ToNumber(left) >> (ushort)JsValue.ToNumber(right);
        }

        public static double Operation_UnsignedRightShift(double left, object right)
        {
            if (right is double)
                return (long)left >> (ushort)(double)right;

            if (JsValue.IsUndefined(right))
                return (long)left;
            return (long)left >> (ushort)JsValue.ToNumber(right);
        }

        public static double Operation_UnsignedRightShift(object left, double right)
        {
            if (left is double)
                return (long)(double)left >> (ushort)right;

            if (JsValue.IsUndefined(left))
                return 0;
            return (long)JsValue.ToNumber(left) >> (ushort)right;
        }

        public object Operation_Member(object obj, object index)
        {
            var @object = obj as JsObject;
            if (@object != null)
                return @object.GetProperty(index);

            if (index is double && JsValue.IsString(obj))
                return obj.ToString().Substring((int)(double)index, 1);

            return GetMemberOnPrototype(obj, index);
        }

        public object Operation_Member(JsObject obj, object index)
        {
            return obj.GetProperty(index);
        }

        public object Operation_Member(object obj, double index)
        {
            var @object = obj as JsObject;
            if (@object != null)
                return Operation_Member(@object, index);

            return Operation_Member(obj, (object)index);
        }

        public object Operation_Member(JsObject obj, double index)
        {
            int intIndex = (int)index;
            if (index == intIndex)
            {
                var arrayStore = obj.PropertyStore as ArrayPropertyStore;
                if (arrayStore != null)
                    return arrayStore.GetOwnProperty(intIndex);
            }

            return obj.GetProperty(index);
        }

        public static object Operation_SetMember(object obj, double index, object value)
        {
            return Operation_SetMember((JsObject)obj, index, value);
        }

        public static object Operation_SetMember(JsObject obj, double index, object value)
        {
            int intIndex = (int)index;
            if (index == intIndex)
            {
                var arrayStore = obj.PropertyStore as ArrayPropertyStore;
                if (arrayStore != null)
                {
                    arrayStore.DefineOrSetPropertyValue(intIndex, value);
                    return value;
                }
            }

            obj.SetProperty(index, value);

            return value;
        }

        public static object Operation_SetMember(object obj, object index, object value)
        {
            ((JsObject)obj).SetProperty(index, value);

            return value;
        }

        public object Operation_Member(object obj, string name)
        {
            if (JsValue.IsUndefined(obj))
            {
                var undefined = (JsUndefined)obj;
                if (undefined.Name != null)
                    name = undefined.Name + "." + name;

                return Global.Engine.ResolveUndefined(name, null);
            }

            var @object = obj as JsObject;
            if (@object != null)
                return @object.GetProperty(name);

            return GetMemberOnPrototype(obj, Global.ResolveIdentifier(name));
        }

        private object GetMemberOnPrototype(object obj, int index)
        {
            var value = Global.GetPrototype(obj).GetPropertyRaw(index);
            if (value == null)
                return JsUndefined.Instance;

            var accessor = value as PropertyAccessor;
            if (accessor != null)
                return accessor.GetValue(obj);

            return value;
        }

        private object GetMemberOnPrototype(object obj, object index)
        {
            var value = Global.GetPrototype(obj).GetPropertyRaw(index);
            if (value == null)
                return JsUndefined.Instance;

            var accessor = value as PropertyAccessor;
            if (accessor != null)
                return accessor.GetValue(obj);

            return value;
        }

        public object GetMember(object obj, string name)
        {
            return Operation_Member(obj, name);
        }

        public object GetMemberByIndex(object obj, int index)
        {
            if (JsValue.IsUndefined(obj))
            {
                string name = Global.GetIdentifier(index);

                var undefined = (JsUndefined)obj;
                if (undefined.Name != null)
                    name = undefined.Name + "." + name;

                return Global.Engine.ResolveUndefined(name, null);
            }

            var @object = obj as JsObject;
            if (@object != null)
                return @object.GetProperty(index);

            return GetMemberOnPrototype(obj, index);
        }

        public bool HasMemberByIndex(object obj, int index)
        {
            var @object = obj as JsObject;
            if (@object != null)
                return @object.HasProperty(index);

            return false;
        }

        public static object Operation_SetMember(object obj, string name, object value)
        {
            return SetMember(obj, name, value);
        }

        public static object SetMember(object obj, string name, object value)
        {
            var @object = obj as JsObject;
            if (@object != null)
                @object.SetProperty(name, value);

            return value;
        }

        public static object SetMemberByIndex(object obj, int index, object value)
        {
            var @object = obj as JsObject;
            if (@object != null)
            {
                @object.SetProperty(index, value);
                return value;
            }

            return value;
        }
    }
}
