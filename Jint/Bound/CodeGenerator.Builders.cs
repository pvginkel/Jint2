using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Jint.Native;

namespace Jint.Bound
{
    partial class CodeGenerator
    {
        private static readonly Dictionary<int, MethodInfo> _operationCache = BuildOperationCache();

        private static Dictionary<int, MethodInfo> BuildOperationCache()
        {
            var result = new Dictionary<int, MethodInfo>();

            const string prefix = "Operation_";

            foreach (var method in typeof(JintRuntime).GetMethods())
            {
                if (method.Name.StartsWith(prefix))
                {
                    var name = method.Name.Substring(prefix.Length);

                    var operation = (Operation)Enum.Parse(typeof(Operation), name);

                    var parameters = method.GetParameters();

                    var a = parameters[0].ParameterType.ToValueType();
                    var b =
                        parameters.Length < 2
                        ? BoundValueType.Unset
                        : parameters[1].ParameterType.ToValueType();
                    var c =
                        parameters.Length < 3
                        ? BoundValueType.Unset
                        : parameters[2].ParameterType.ToValueType();

                    result[GetOperationMethodKey(operation, a, b, c)] = method;
                }
            }

            return result;
        }

        private BoundValueType EmitOperationCall(Operation operation, BoundExpression obj, BoundExpression index, BoundExpression value)
        {
            var indexType = index.ValueType;

            bool boxIndex = false;

            var method = FindOperationMethod(operation, BoundValueType.Unknown, indexType, BoundValueType.Unknown);
            var builder = FindOperationBuilder(operation, BoundValueType.Unknown, indexType, BoundValueType.Unknown);

            if (method == null && builder == null)
            {
                method = FindOperationMethod(operation, BoundValueType.Unknown, BoundValueType.Unknown, BoundValueType.Unknown);
                builder = FindOperationBuilder(operation, BoundValueType.Unknown, BoundValueType.Unknown, BoundValueType.Unknown);

                boxIndex = true;
            }

            if (builder != null)
                return builder(this, new[] { obj, index, value });

            if (!method.IsStatic)
                _scope.EmitLoad(SpecialLocal.Runtime);

            EmitBox(EmitExpression(obj));

            EmitExpression(index);
            if (boxIndex)
                EmitBox(indexType);

            EmitBox(EmitExpression(value));

            return IL.EmitCall(method);
        }

        private BoundValueType EmitOperationCall(Operation operation, BoundExpression left, BoundExpression right)
        {
            var leftType = left.ValueType;
            var rightType = right.ValueType;

            var method = FindOperationMethod(operation, leftType, rightType);
            var builder = FindOperationBuilder(operation, leftType, rightType);

            bool boxLeft = false;
            bool boxRight = false;

            if (method == null && builder == null)
            {
                method = FindOperationMethod(operation, leftType, BoundValueType.Unknown);
                builder = FindOperationBuilder(operation, leftType, BoundValueType.Unknown);

                if (method != null || builder != null)
                    boxRight = true;
            }

            if (method == null && builder == null)
            {
                method = FindOperationMethod(operation, BoundValueType.Unknown, rightType);
                builder = FindOperationBuilder(operation, BoundValueType.Unknown, rightType);

                if (method != null || builder != null)
                    boxLeft = true;
            }

            if (method == null && builder == null)
            {
                method = FindOperationMethod(operation, BoundValueType.Unknown, BoundValueType.Unknown);
                builder = FindOperationBuilder(operation, BoundValueType.Unknown, BoundValueType.Unknown);
                boxLeft = true;
                boxRight = true;
            }

            if (builder != null)
                return builder(this, new[] { left, right });

            if (!method.IsStatic)
                _scope.EmitLoad(SpecialLocal.Runtime);

            EmitExpression(left);
            if (boxLeft)
                EmitBox(leftType);
            EmitExpression(right);
            if (boxRight)
                EmitBox(rightType);

            return IL.EmitCall(method);
        }

        private BoundValueType EmitOperationCall(Operation operation, BoundExpression operand)
        {
            var operandType = operand.ValueType;

            var method = FindOperationMethod(operation, operandType);
            var builder = FindOperationBuilder(operation, operandType);

            bool boxOperand = false;

            if (method == null && builder == null)
            {
                method = FindOperationMethod(operation, BoundValueType.Unknown);
                builder = FindOperationBuilder(operation, BoundValueType.Unknown);
                boxOperand = true;
            }

            if (builder != null)
                return builder(this, new[] { operand });

            if (!method.IsStatic)
                _scope.EmitLoad(SpecialLocal.Runtime);

            EmitExpression(operand);
            if (boxOperand)
                EmitBox(operandType);

            return IL.EmitCall(method);
        }

        private static MethodInfo FindOperationMethod(Operation operation, BoundValueType operand)
        {
            return FindOperationMethod(operation, operand, BoundValueType.Unset);
        }

        private static MethodInfo FindOperationMethod(Operation operation, BoundValueType left, BoundValueType right)
        {
            return FindOperationMethod(operation, left, right, BoundValueType.Unset);
        }

        private static MethodInfo FindOperationMethod(Operation operation, BoundValueType a, BoundValueType b, BoundValueType c)
        {
            MethodInfo result;
            _operationCache.TryGetValue(GetOperationMethodKey(operation, a, b, c), out result);
            return result;
        }

        private static int GetOperationMethodKey(Operation operation, BoundValueType a)
        {
            return GetOperationMethodKey(operation, a, BoundValueType.Unset);
        }

        private static int GetOperationMethodKey(Operation operation, BoundValueType a, BoundValueType b)
        {
            return GetOperationMethodKey(operation, a, b, BoundValueType.Unset);
        }

        private static int GetOperationMethodKey(Operation operation, BoundValueType a, BoundValueType b, BoundValueType c)
        {
            return (int)operation << 24 | (int)a << 16 | (int)b << 8 | (int)c;
        }

        private enum Operation
        {
            Add,
            BitwiseAnd,
            BitwiseExclusiveOr,
            BitwiseNot,
            BitwiseOr,
            Delete,
            Divide,
            Equal,
            GreaterThan,
            GreaterThanOrEqual,
            In,
            InstanceOf,
            LeftShift,
            LessThan,
            LessThanOrEqual,
            Member,
            Modulo,
            Multiply,
            Negate,
            Not,
            NotEqual,
            NotSame,
            RightShift,
            Same,
            SetMember,
            Subtract,
            TypeOf,
            UnaryPlus,
            UnsignedRightShift
        }
    }
}
