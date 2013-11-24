using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Jint.Native;

namespace Jint.Runtime
{
    partial class JintRuntime
    {
        public static JsInstance Operation_Add(JsInstance left, JsInstance right)
        {
            var leftPrimitive = left.ToPrimitive(PrimitiveHint.None);
            var rightPrimitive = right.ToPrimitive(PrimitiveHint.None);

            if (leftPrimitive is JsString || rightPrimitive is JsString)
                return JsString.Create(leftPrimitive.ToString() + rightPrimitive.ToString());

            return JsNumber.Create(leftPrimitive.ToNumber() + rightPrimitive.ToNumber());
        }

        public static string Operation_Add(string left, JsInstance right)
        {
            return left + right.ToPrimitive(PrimitiveHint.None).ToString();
        }

        public static string Operation_Add(JsInstance left, string right)
        {
            return left.ToPrimitive(PrimitiveHint.None).ToString() + right;
        }

        public static JsInstance Operation_Add(double left, JsInstance right)
        {
            var rightPrimitive = right.ToPrimitive(PrimitiveHint.None);

            if (rightPrimitive is JsString)
                return JsString.Create(left.ToString(CultureInfo.InvariantCulture) + rightPrimitive.ToString());

            return JsNumber.Create(left + rightPrimitive.ToNumber());
        }

        public static JsInstance Operation_Add(JsInstance left, double right)
        {
            var leftPrimitive = left.ToPrimitive(PrimitiveHint.None);

            if (leftPrimitive is JsString)
                return JsString.Create(leftPrimitive.ToString() + right.ToString(CultureInfo.InvariantCulture));

            return JsNumber.Create(leftPrimitive.ToNumber() + right);
        }

        public static double Operation_BitwiseAnd(JsInstance left, JsInstance right)
        {
            if (JsInstance.IsUndefined(left) || JsInstance.IsUndefined(right))
                return 0;

            return (long)left.ToNumber() & (long)right.ToNumber();
        }

        public static double Operation_BitwiseAnd(double left, JsInstance right)
        {
            if (JsInstance.IsUndefined(right))
                return 0;

            return (long)left & (long)right.ToNumber();
        }

        public static double Operation_BitwiseAnd(JsInstance left, double right)
        {
            if (JsInstance.IsUndefined(left))
                return 0;

            return (long)left.ToNumber() & (long)right;
        }

        public static double Operation_BitwiseExclusiveOr(JsInstance left, JsInstance right)
        {
            if (JsInstance.IsUndefined(left))
            {
                if (JsInstance.IsUndefined(right))
                    return 1;

                return (long)right.ToNumber();
            }

            if (JsInstance.IsUndefined(right))
                return (long)left.ToNumber();

            return (long)left.ToNumber() ^ (long)right.ToNumber();
        }

        public static double Operation_BitwiseExclusiveOr(double left, JsInstance right)
        {
            if (JsInstance.IsUndefined(right))
                return (long)left;

            return (long)left ^ (long)right.ToNumber();
        }

        public static double Operation_BitwiseExclusiveOr(JsInstance left, double right)
        {
            if (JsInstance.IsUndefined(left))
                return (long)right;

            return (long)left.ToNumber() ^ (long)right;
        }

        public static double Operation_BitwiseNot(JsInstance operand)
        {
            var number = operand.ToPrimitive(PrimitiveHint.None).ToNumber();

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

        public static double Operation_BitwiseOr(JsInstance left, JsInstance right)
        {
            if (JsInstance.IsUndefined(left))
            {
                if (JsInstance.IsUndefined(right))
                    return 1;

                return (long)right.ToNumber();
            }

            if (JsInstance.IsUndefined(right))
                return (long)left.ToNumber();

            return (long)left.ToNumber() | (long)right.ToNumber();
        }

        public static double Operation_BitwiseOr(double left, JsInstance right)
        {
            if (JsInstance.IsUndefined(right))
                return (long)left;

            return (long)left | (long)right.ToNumber();
        }

        public static double Operation_BitwiseOr(JsInstance left, double right)
        {
            if (JsInstance.IsUndefined(left))
                return (long)right;

            return (long)left.ToNumber() | (long)right;
        }

        public static double Operation_Divide(JsInstance left, JsInstance right)
        {
            return Operation_Divide(left.ToNumber(), right.ToNumber());
        }

        public static double Operation_Divide(double left, JsInstance right)
        {
            return Operation_Divide(left, right.ToNumber());
        }

        public static double Operation_Divide(JsInstance left, double right)
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

        public bool Operation_In(JsInstance left, JsInstance right)
        {
            if (right is ILiteral)
                throw new JsException(_errorClass.New("Cannot apply 'in' operator to the specified member."));

            return ((JsObject)right).HasProperty(left);
        }

        public bool Operation_InstanceOf(JsInstance left, JsInstance right)
        {
            var function = right as JsFunction;
            var obj = left as JsObject;

            if (function == null)
                throw new JsException(_typeErrorClass.New("Right argument should be a function"));
            if (obj == null)
                throw new JsException(_typeErrorClass.New("Left argument should be an object"));

            return function.HasInstance(obj);
        }

        public static double Operation_LeftShift(JsInstance left, JsInstance right)
        {
            if (JsInstance.IsUndefined(left))
                return 0;
            if (JsInstance.IsUndefined(right))
                return (long)left.ToNumber();
            return (long)left.ToNumber() << (ushort)right.ToNumber();
        }

        public static double Operation_LeftShift(double left, JsInstance right)
        {
            if (JsInstance.IsUndefined(right))
                return (long)left;
            return (long)left << (ushort)right.ToNumber();
        }

        public static double Operation_LeftShift(JsInstance left, double right)
        {
            if (JsInstance.IsUndefined(left))
                return 0;
            return (long)left.ToNumber() << (ushort)right;
        }

        public static double Operation_Modulo(JsInstance left, JsInstance right)
        {
            double rightNumber = right.ToNumber();
            if (Double.IsInfinity(rightNumber))
                return Double.PositiveInfinity;
            if (rightNumber == 0)
                return Double.NaN;
            return left.ToNumber() % rightNumber;
        }

        public static double Operation_Modulo(double left, JsInstance right)
        {
            double rightNumber = right.ToNumber();
            if (Double.IsInfinity(rightNumber))
                return Double.PositiveInfinity;
            return left % rightNumber;
        }

        public static double Operation_Modulo(JsInstance left, double right)
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

        public static double Operation_Multiply(JsInstance left, JsInstance right)
        {
            return left.ToNumber() * right.ToNumber();
        }

        public static double Operation_Multiply(double left, JsInstance right)
        {
            return left * right.ToNumber();
        }

        public static double Operation_Multiply(JsInstance left, double right)
        {
            return left.ToNumber() * right;
        }

        public static double Operation_Multiply(double left, double right)
        {
            return left * right;
        }

        public static double Operation_Negate(JsInstance operand)
        {
            return -operand.ToNumber();
        }

        public static bool Operation_Not(JsInstance operand)
        {
            return !operand.ToBoolean();
        }

        public static double Operation_Power(JsInstance left, JsInstance right)
        {
            return Math.Pow(left.ToNumber(), right.ToNumber());
        }

        public static double Operation_Power(double left, JsInstance right)
        {
            return Math.Pow(left, right.ToNumber());
        }

        public static double Operation_Power(JsInstance left, double right)
        {
            return Math.Pow(left.ToNumber(), right);
        }

        public static double Operation_RightShift(JsInstance left, JsInstance right)
        {
            if (JsInstance.IsUndefined(left))
                return 0;
            if (JsInstance.IsUndefined(right))
                return (long)left.ToNumber();
            return (long)left.ToNumber() >> (ushort)right.ToNumber();
        }

        public static double Operation_RightShift(double left, JsInstance right)
        {
            if (JsInstance.IsUndefined(right))
                return (long)left;
            return (long)left >> (ushort)right.ToNumber();
        }

        public static double Operation_RightShift(JsInstance left, double right)
        {
            if (JsInstance.IsUndefined(left))
                return 0;
            return (long)left.ToNumber() >> (ushort)right;
        }

        public static double Operation_Subtract(JsInstance left, JsInstance right)
        {
            return left.ToNumber() - right.ToNumber();
        }

        public static double Operation_Subtract(double left, JsInstance right)
        {
            return left - right.ToNumber();
        }

        public static double Operation_Subtract(JsInstance left, double right)
        {
            return left.ToNumber() - right;
        }

        public static string Operation_TypeOf(JsInstance operand)
        {
            if (operand == null)
                return JsInstance.TypeUndefined;
            if (operand is JsNull)
                return JsInstance.TypeObject;
            if (operand is JsFunction)
                return JsInstance.TypeFunction;
            switch (operand.Type)
            {
                case JsType.Boolean: return JsInstance.TypeBoolean;
                case JsType.Number: return JsInstance.TypeNumber;
                case JsType.Object: return JsInstance.TypeObject;
                case JsType.String: return JsInstance.TypeString;
                case JsType.Undefined: return JsInstance.TypeUndefined;
                default: throw new InvalidOperationException();
            }
        }

        public static string Operation_TypeOf(JsObject scope, string identifier)
        {
            Descriptor descriptor;
            if (!scope.TryGetDescriptor(identifier, out descriptor))
                return JsInstance.TypeUndefined;

            return Operation_TypeOf(descriptor.Get(scope));
        }

        public static double Operation_UnaryPlus(JsInstance operand)
        {
            return operand.ToNumber();
        }

        public static double Operation_UnsignedRightShift(JsInstance left, JsInstance right)
        {
            if (JsInstance.IsUndefined(left))
                return 0;
            if (JsInstance.IsUndefined(right))
                return (long)left.ToNumber();
            return (long)left.ToNumber() >> (ushort)right.ToNumber();
        }

        public static double Operation_UnsignedRightShift(double left, JsInstance right)
        {
            if (JsInstance.IsUndefined(right))
                return (long)left;
            return (long)left >> (ushort)right.ToNumber();
        }

        public static double Operation_UnsignedRightShift(JsInstance left, double right)
        {
            if (JsInstance.IsUndefined(left))
                return 0;
            return (long)left.ToNumber() >> (ushort)right;
        }

        public JsInstance Operation_Index(JsInstance obj, JsInstance index)
        {
            var stringObj = obj as JsString;
            var numberIndex = index as JsNumber;

            if (stringObj != null && numberIndex != null)
            {
                return JsString.Create(
                    ((string)stringObj.Value).Substring((int)numberIndex.ToNumber(), 1)
                );
            }

            var jsObject = obj as JsObject;
            if (jsObject == null)
                jsObject = Global.GetPrototype(obj);

            return jsObject[index];
        }

        public JsInstance Operation_Index(JsInstance obj, double index)
        {
            var array = obj as JsArray;
            if (array != null)
            {
                int intIndex = (int)index;
                if (index == intIndex)
                    return array.Get(intIndex);
            }

            return Operation_Index(obj, JsNumber.Create(index));
        }

        public static JsInstance Operation_SetIndex(JsInstance obj, double index, JsInstance value)
        {
            var array = obj as JsArray;
            if (array != null)
            {
                int intIndex = (int)index;
                if (index == intIndex)
                    return array.Put(intIndex, value);
            }

            return ((JsObject)obj)[JsNumber.Create(index)] = value;
        }

        public JsInstance Operation_Member(JsInstance obj, string name)
        {
            var undefined = obj as JsUndefined;
            if (undefined != null)
            {
                if (undefined.Name != null)
                    name = undefined.Name + "." + name;

                return Global.Backend.ResolveUndefined(name, null);
            }

            var jsObject = obj as JsObject;
            if (jsObject == null)
            {
                jsObject = Global.GetPrototype(obj);
                var descriptor = jsObject.GetDescriptor(name);
                if (descriptor == null)
                    return JsUndefined.Instance;

                return descriptor.Get(obj);
            }

            return jsObject[name];
        }

        public static JsInstance Operation_SetMember(JsInstance obj, string name, JsInstance value)
        {
            var dictionary = obj as JsObject;

            if (dictionary != null)
                return dictionary[name] = value;

            return value;
        }
    }
}
