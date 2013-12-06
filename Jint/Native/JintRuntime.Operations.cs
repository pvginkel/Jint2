// ReSharper disable CompareOfFloatsByEqualityOperator

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JintRuntime
    {
        public static JsBox Operation_Add(JsBox left, JsBox right)
        {
            var leftPrimitive = left.ToPrimitive();
            var rightPrimitive = right.ToPrimitive();

            if (leftPrimitive.IsString || rightPrimitive.IsString)
                return JsString.Box(leftPrimitive.ToString() + rightPrimitive.ToString());

            return JsNumber.Box(leftPrimitive.ToNumber() + rightPrimitive.ToNumber());
        }

        public static string Operation_Add(string left, JsBox right)
        {
            return left + right.ToPrimitive().ToString();
        }

        public static string Operation_Add(JsBox left, string right)
        {
            return left.ToPrimitive().ToString() + right;
        }

        public static JsBox Operation_Add(double left, JsBox right)
        {
            var rightPrimitive = right.ToPrimitive();

            if (rightPrimitive.IsString)
                return JsString.Box(left.ToString(CultureInfo.InvariantCulture) + rightPrimitive.ToString());

            return JsNumber.Box(left + rightPrimitive.ToNumber());
        }

        public static JsBox Operation_Add(JsBox left, double right)
        {
            var leftPrimitive = left.ToPrimitive();

            if (leftPrimitive.IsString)
                return JsString.Box(leftPrimitive.ToString() + right.ToString(CultureInfo.InvariantCulture));

            return JsNumber.Box(leftPrimitive.ToNumber() + right);
        }

        public static double Operation_BitwiseAnd(JsBox left, JsBox right)
        {
            if (left.IsUndefined || right.IsUndefined)
                return 0;

            return (long)left.ToNumber() & (long)right.ToNumber();
        }

        public static double Operation_BitwiseAnd(double left, JsBox right)
        {
            if (right.IsUndefined)
                return 0;

            return (long)left & (long)right.ToNumber();
        }

        public static double Operation_BitwiseAnd(JsBox left, double right)
        {
            if (left.IsUndefined)
                return 0;

            return (long)left.ToNumber() & (long)right;
        }

        public static double Operation_BitwiseExclusiveOr(JsBox left, JsBox right)
        {
            if (left.IsUndefined)
            {
                if (right.IsUndefined)
                    return 1;

                return (long)right.ToNumber();
            }

            if (right.IsUndefined)
                return (long)left.ToNumber();

            return (long)left.ToNumber() ^ (long)right.ToNumber();
        }

        public static double Operation_BitwiseExclusiveOr(double left, JsBox right)
        {
            if (right.IsUndefined)
                return (long)left;

            return (long)left ^ (long)right.ToNumber();
        }

        public static double Operation_BitwiseExclusiveOr(JsBox left, double right)
        {
            if (left.IsUndefined)
                return (long)right;

            return (long)left.ToNumber() ^ (long)right;
        }

        public static double Operation_BitwiseNot(JsBox operand)
        {
            var number = operand.ToPrimitive().ToNumber();

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

        public static double Operation_BitwiseOr(JsBox left, JsBox right)
        {
            if (left.IsUndefined)
            {
                if (right.IsUndefined)
                    return 1;

                return (long)right.ToNumber();
            }

            if (right.IsUndefined)
                return (long)left.ToNumber();

            return (long)left.ToNumber() | (long)right.ToNumber();
        }

        public static double Operation_BitwiseOr(double left, JsBox right)
        {
            if (right.IsUndefined)
                return (long)left;

            return (long)left | (long)right.ToNumber();
        }

        public static double Operation_BitwiseOr(JsBox left, double right)
        {
            if (left.IsUndefined)
                return (long)right;

            return (long)left.ToNumber() | (long)right;
        }

        public static double Operation_Divide(JsBox left, JsBox right)
        {
            return Operation_Divide(left.ToNumber(), right.ToNumber());
        }

        public static double Operation_Divide(double left, JsBox right)
        {
            return Operation_Divide(left, right.ToNumber());
        }

        public static double Operation_Divide(JsBox left, double right)
        {
            return Operation_Divide(left.ToNumber(), right);
        }

        public static double Operation_Divide(double left, double right)
        {
            if (Double.IsInfinity(right))
                return 0;

            if (right == 0)
                return left > 0 ? Double.PositiveInfinity : Double.NegativeInfinity;

            return left / right;
        }

        public bool Operation_In(JsBox left, JsBox right)
        {
            if (right.IsLiteral)
                throw new JsException(JsErrorType.Error, "Cannot apply 'in' operator to the specified member.");

            return ((JsObject)right).HasProperty(left);
        }

        public bool Operation_InstanceOf(JsBox left, JsBox right)
        {
            if (!right.IsFunction)
                throw new JsException(JsErrorType.TypeError, "Right argument should be a function");
            if (!left.IsObject)
                throw new JsException(JsErrorType.TypeError, "Left argument should be an object");

            return ((JsObject)right).HasInstance((JsObject)left);
        }

        public static double Operation_LeftShift(JsBox left, JsBox right)
        {
            if (left.IsUndefined)
                return 0;
            if (right.IsUndefined)
                return (long)left.ToNumber();
            return (long)left.ToNumber() << (ushort)right.ToNumber();
        }

        public static double Operation_LeftShift(double left, JsBox right)
        {
            if (right.IsUndefined)
                return (long)left;
            return (long)left << (ushort)right.ToNumber();
        }

        public static double Operation_LeftShift(JsBox left, double right)
        {
            if (left.IsUndefined)
                return 0;
            return (long)left.ToNumber() << (ushort)right;
        }

        public static double Operation_Modulo(JsBox left, JsBox right)
        {
            double rightNumber = right.ToNumber();
            if (Double.IsInfinity(rightNumber))
                return Double.PositiveInfinity;
            if (rightNumber == 0)
                return Double.NaN;
            return left.ToNumber() % rightNumber;
        }

        public static double Operation_Modulo(double left, JsBox right)
        {
            double rightNumber = right.ToNumber();
            if (Double.IsInfinity(rightNumber))
                return Double.PositiveInfinity;
            return left % rightNumber;
        }

        public static double Operation_Modulo(JsBox left, double right)
        {
            if (Double.IsInfinity(right))
                return Double.PositiveInfinity;
            if (right == 0)
                return Double.NaN;
            return left.ToNumber() % right;
        }

        public static double Operation_Modulo(double left, double right)
        {
            if (Double.IsInfinity(right))
                return Double.PositiveInfinity;
            if (right == 0)
                return Double.NaN;
            return left % right;
        }

        public static double Operation_Multiply(JsBox left, JsBox right)
        {
            return left.ToNumber() * right.ToNumber();
        }

        public static double Operation_Multiply(double left, JsBox right)
        {
            return left * right.ToNumber();
        }

        public static double Operation_Multiply(JsBox left, double right)
        {
            return left.ToNumber() * right;
        }

        public static double Operation_Multiply(double left, double right)
        {
            return left * right;
        }

        public static double Operation_Negate(JsBox operand)
        {
            return -operand.ToNumber();
        }

        public static bool Operation_Not(JsBox operand)
        {
            return !operand.ToBoolean();
        }

        public static double Operation_Power(JsBox left, JsBox right)
        {
            return Math.Pow(left.ToNumber(), right.ToNumber());
        }

        public static double Operation_Power(double left, JsBox right)
        {
            return Math.Pow(left, right.ToNumber());
        }

        public static double Operation_Power(JsBox left, double right)
        {
            return Math.Pow(left.ToNumber(), right);
        }

        public static double Operation_RightShift(JsBox left, JsBox right)
        {
            if (left.IsUndefined)
                return 0;
            if (right.IsUndefined)
                return (long)left.ToNumber();
            return (long)left.ToNumber() >> (ushort)right.ToNumber();
        }

        public static double Operation_RightShift(double left, JsBox right)
        {
            if (right.IsUndefined)
                return (long)left;
            return (long)left >> (ushort)right.ToNumber();
        }

        public static double Operation_RightShift(JsBox left, double right)
        {
            if (left.IsUndefined)
                return 0;
            return (long)left.ToNumber() >> (ushort)right;
        }

        public static double Operation_Subtract(JsBox left, JsBox right)
        {
            return left.ToNumber() - right.ToNumber();
        }

        public static double Operation_Subtract(double left, JsBox right)
        {
            return left - right.ToNumber();
        }

        public static double Operation_Subtract(JsBox left, double right)
        {
            return left.ToNumber() - right;
        }

        public static string Operation_TypeOf(JsBox operand)
        {
            return operand.GetTypeOf();
        }

        public static string Operation_TypeOf(JsObject scope, string identifier)
        {
            Descriptor descriptor;
            if (!scope.TryGetDescriptor(scope.Global.ResolveIdentifier(identifier), out descriptor))
                return JsNames.TypeUndefined;

            return Operation_TypeOf(descriptor.Get(JsBox.CreateObject(scope)));
        }

        public static double Operation_UnaryPlus(JsBox operand)
        {
            return operand.ToNumber();
        }

        public static double Operation_UnsignedRightShift(JsBox left, JsBox right)
        {
            if (left.IsUndefined)
                return 0;
            if (right.IsUndefined)
                return (long)left.ToNumber();
            return (long)left.ToNumber() >> (ushort)right.ToNumber();
        }

        public static double Operation_UnsignedRightShift(double left, JsBox right)
        {
            if (right.IsUndefined)
                return (long)left;
            return (long)left >> (ushort)right.ToNumber();
        }

        public static double Operation_UnsignedRightShift(JsBox left, double right)
        {
            if (left.IsUndefined)
                return 0;
            return (long)left.ToNumber() >> (ushort)right;
        }

        public JsBox Operation_Index(JsBox obj, JsBox index)
        {
            if (obj.IsObject)
                return ((JsObject)obj)[index];

            if (obj.IsString && index.IsNumber)
            {
                return JsString.Box(
                    ((string)obj).Substring((int)(double)index, 1)
                );
            }

            return Global.GetPrototype(obj)[index];
        }

        public JsBox Operation_Index(JsObject obj, JsBox index)
        {
            return obj[index];
        }

        public JsBox Operation_Index(JsBox obj, double index)
        {
            if (obj.IsObject)
                return Operation_Index((JsObject)obj, index);

            return Operation_Index(obj, JsNumber.Box(index));
        }

        public JsBox Operation_Index(JsObject obj, double index)
        {
            int intIndex = (int)index;
            if (index == intIndex)
            {
                var arrayStore = obj.PropertyStore as ArrayPropertyStore;
                if (arrayStore != null)
                    return arrayStore[intIndex];
            }

            return obj[JsNumber.Box(index)];
        }

        public static JsBox Operation_SetIndex(JsBox obj, double index, JsBox value)
        {
            return Operation_SetIndex((JsObject)obj, index, value);
        }

        public static JsBox Operation_SetIndex(JsObject obj, double index, JsBox value)
        {
            int intIndex = (int)index;
            if (index == intIndex)
            {
                var arrayStore = obj.PropertyStore as ArrayPropertyStore;
                if (arrayStore != null)
                    return arrayStore[intIndex] = value;
            }

            return obj[JsNumber.Box(index)] = value;
        }

        public static JsBox Operation_SetIndex(JsBox obj, JsBox index, JsBox value)
        {
            return ((JsObject)obj)[index] = value;
        }

        public JsBox Operation_Member(JsBox obj, string name)
        {
            if (obj.IsUndefined)
            {
                var undefined = (JsUndefined)obj.ToInstance();
                if (undefined.Name != null)
                    name = undefined.Name + "." + name;

                return Global.Engine.ResolveUndefined(name, null);
            }

            if (!obj.IsObject)
            {
                var jsObject = Global.GetPrototype(obj);
                var descriptor = jsObject.GetDescriptor(Global.ResolveIdentifier(name));
                if (descriptor == null)
                    return JsBox.Undefined;

                return descriptor.Get(obj);
            }

            return ((JsObject)obj)[name];
        }

        public JsBox GetMember(JsBox obj, string name)
        {
            return Operation_Member(obj, name);
        }

        public JsBox GetMemberByIndex(JsBox obj, int index)
        {
            if (obj.IsUndefined)
            {
                string name = Global.GetIdentifier(index);

                var undefined = (JsUndefined)obj.ToInstance();
                if (undefined.Name != null)
                    name = undefined.Name + "." + name;

                return Global.Engine.ResolveUndefined(name, null);
            }

            if (obj.IsObject)
                return ((JsObject)obj).GetProperty(index);

            var prototype = Global.GetPrototype(obj);
            var descriptor = prototype.GetDescriptor(index);
            if (descriptor == null)
                return JsBox.Undefined;

            return descriptor.Get(JsBox.CreateObject(prototype));
        }

        public static JsBox Operation_SetMember(JsBox obj, string name, JsBox value)
        {
            return SetMember(obj, name, value);
        }

        public static JsBox SetMember(JsBox obj, string name, JsBox value)
        {
            if (obj.IsObject)
                ((JsObject)obj)[name] = value;

            return value;
        }

        public static JsBox SetMemberByIndex(JsBox obj, int index, JsBox value)
        {
            if (obj.IsObject)
            {
                ((JsObject)obj).SetProperty(index, value);
                return value;
            }

            return value;
        }
    }
}
