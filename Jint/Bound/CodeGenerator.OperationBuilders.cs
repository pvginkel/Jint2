using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Jint.Native;

namespace Jint.Bound
{
    partial class CodeGenerator
    {
        private static readonly Dictionary<int, Func<CodeGenerator, BoundExpression[], BoundValueType>> _operationBuilders = CreateOperationBuilders();

        private static Dictionary<int, Func<CodeGenerator, BoundExpression[], BoundValueType>> CreateOperationBuilders()
        {
            var result = new Dictionary<int, Func<CodeGenerator, BoundExpression[], BoundValueType>>();

            result[GetOperationMethodKey(Operation.Add, BoundValueType.String, BoundValueType.String)] =
                (generator, arguments) =>
                {
                    generator.EmitExpressions(arguments);
                    return generator.IL.EmitCall(_concat);
                };

            result[GetOperationMethodKey(Operation.Add, BoundValueType.Number, BoundValueType.Number)] =
                (generator, arguments) => generator.EmitNumberOperation(OpCodes.Add, arguments);

            result[GetOperationMethodKey(Operation.Subtract, BoundValueType.Number, BoundValueType.Number)] =
                (generator, arguments) => generator.EmitNumberOperation(OpCodes.Sub, arguments);

            result[GetOperationMethodKey(Operation.BitwiseAnd, BoundValueType.Number, BoundValueType.Number)] =
                (generator, arguments) => generator.EmitBitwiseOperation(OpCodes.And, arguments);

            result[GetOperationMethodKey(Operation.BitwiseExclusiveOr, BoundValueType.Number, BoundValueType.Number)] =
                (generator, arguments) => generator.EmitBitwiseOperation(OpCodes.Xor, arguments);

            result[GetOperationMethodKey(Operation.BitwiseOr, BoundValueType.Number, BoundValueType.Number)] =
                (generator, arguments) => generator.EmitBitwiseOperation(OpCodes.Or, arguments);

            result[GetOperationMethodKey(Operation.LeftShift, BoundValueType.Number, BoundValueType.Number)] =
                (generator, arguments) => generator.EmitBitwiseShiftOperation(OpCodes.Shl, arguments);

            result[GetOperationMethodKey(Operation.RightShift, BoundValueType.Number, BoundValueType.Number)] =
                (generator, arguments) => generator.EmitBitwiseShiftOperation(OpCodes.Shr, arguments);

            result[GetOperationMethodKey(Operation.UnsignedRightShift, BoundValueType.Number, BoundValueType.Number)] =
                (generator, arguments) => generator.EmitBitwiseShiftOperation(OpCodes.Shr_Un, arguments);

            result[GetOperationMethodKey(Operation.Multiply, BoundValueType.Number, BoundValueType.Number)] =
                (generator, arguments) => generator.EmitNumberOperation(OpCodes.Mul, arguments);

            result[GetOperationMethodKey(Operation.Negate, BoundValueType.Number)] =
                (generator, arguments) => generator.EmitNumberOperation(OpCodes.Neg, arguments);

            result[GetOperationMethodKey(Operation.Not, BoundValueType.Boolean)] =
                (generator, arguments) =>
                {
                    generator.EmitExpressions(arguments);
                    generator.IL.EmitNot();
                    return BoundValueType.Boolean;
                };

            result[GetOperationMethodKey(Operation.TypeOf, BoundValueType.String)] =
                (generator, arguments) => generator.IL.EmitConstant(JsNames.TypeString);

            result[GetOperationMethodKey(Operation.TypeOf, BoundValueType.Number)] =
                (generator, arguments) => generator.IL.EmitConstant(JsNames.TypeNumber);

            result[GetOperationMethodKey(Operation.TypeOf, BoundValueType.Boolean)] =
                (generator, arguments) => generator.IL.EmitConstant(JsNames.TypeBoolean);

            result[GetOperationMethodKey(Operation.UnaryPlus, BoundValueType.Number)] =
                (generator, arguments) =>
                {
                    generator.EmitExpressions(arguments);
                    return BoundValueType.Number;
                };

            result[GetOperationMethodKey(Operation.Member, BoundValueType.String, BoundValueType.Number)] =
                (generator, arguments) =>
                {
                    generator.EmitExpression(arguments[0]);
                    if (generator.EmitExpression(arguments[1]) == BoundValueType.Unknown)
                        generator.IL.Emit(OpCodes.Unbox_Any, typeof(double));
                    generator.IL.Emit(OpCodes.Conv_I4);
                    generator.IL.EmitConstant(1);
                    return generator.IL.EmitCall(_substring);
                };

            result[GetOperationMethodKey(Operation.Delete, BoundValueType.Unknown, BoundValueType.String)] =
                (generator, arguments) =>
                {
                    generator.EmitBox(generator.EmitExpression(arguments[0]));
                    generator.IL.Emit(OpCodes.Castclass, typeof(JsObject));
                    generator.EmitExpression(arguments[1]);
                    generator.IL.EmitConstant(generator._scope.IsStrict);
                    return generator.IL.EmitCall(_deleteByString);
                };

            result[GetOperationMethodKey(Operation.Delete, BoundValueType.Unknown, BoundValueType.Unknown)] =
                (generator, arguments) =>
                {
                    generator.EmitBox(generator.EmitExpression(arguments[0]));
                    generator.IL.Emit(OpCodes.Castclass, typeof(JsObject));
                    generator.EmitBox(generator.EmitExpression(arguments[1]));
                    generator.IL.EmitConstant(generator._scope.IsStrict);
                    return generator.IL.EmitCall(_deleteByInstance);
                };

            result[GetOperationMethodKey(Operation.Equal, BoundValueType.Boolean, BoundValueType.Boolean)] =
                (generator, arguments) => generator.EmitCompare(OpCodes.Ceq, arguments);

            result[GetOperationMethodKey(Operation.Equal, BoundValueType.Number, BoundValueType.Number)] =
                (generator, arguments) => generator.EmitCompare(OpCodes.Ceq, arguments);

            result[GetOperationMethodKey(Operation.Equal, BoundValueType.String, BoundValueType.String)] =
                (generator, arguments) =>
                {
                    generator.EmitExpressions(arguments);
                    return generator.IL.EmitCall(_stringEquals);
                };

            result[GetOperationMethodKey(Operation.Same, BoundValueType.Boolean, BoundValueType.Boolean)] =
                (generator, arguments) => generator.EmitCompare(OpCodes.Ceq, arguments);

            result[GetOperationMethodKey(Operation.Same, BoundValueType.Number, BoundValueType.Number)] =
                (generator, arguments) => generator.EmitCompare(OpCodes.Ceq, arguments);

            result[GetOperationMethodKey(Operation.Same, BoundValueType.String, BoundValueType.String)] =
                (generator, arguments) =>
                {
                    generator.EmitExpressions(arguments);
                    return generator.IL.EmitCall(_stringEquals);
                };

            result[GetOperationMethodKey(Operation.NotEqual, BoundValueType.Boolean, BoundValueType.Boolean)] =
                (generator, arguments) => generator.EmitCompare(OpCodes.Ceq, arguments, true);

            result[GetOperationMethodKey(Operation.NotEqual, BoundValueType.Number, BoundValueType.Number)] =
                (generator, arguments) => generator.EmitCompare(OpCodes.Ceq, arguments, true);

            result[GetOperationMethodKey(Operation.NotEqual, BoundValueType.String, BoundValueType.String)] =
                (generator, arguments) =>
                {
                    generator.EmitExpressions(arguments);
                    generator.IL.EmitCall(_stringEquals);
                    generator.IL.EmitNot();
                    return BoundValueType.Boolean;
                };

            result[GetOperationMethodKey(Operation.NotSame, BoundValueType.Boolean, BoundValueType.Boolean)] =
                (generator, arguments) => generator.EmitCompare(OpCodes.Ceq, arguments, true);

            result[GetOperationMethodKey(Operation.NotSame, BoundValueType.Number, BoundValueType.Number)] =
                (generator, arguments) => generator.EmitCompare(OpCodes.Ceq, arguments, true);

            result[GetOperationMethodKey(Operation.NotSame, BoundValueType.String, BoundValueType.String)] =
                (generator, arguments) =>
                {
                    generator.EmitExpressions(arguments);
                    generator.IL.EmitCall(_stringEquals);
                    generator.IL.EmitNot();
                    return BoundValueType.Boolean;
                };

            result[GetOperationMethodKey(Operation.LessThan, BoundValueType.Number, BoundValueType.Number)] =
                (generator, arguments) => generator.EmitCompare(OpCodes.Clt, arguments);

            result[GetOperationMethodKey(Operation.LessThanOrEqual, BoundValueType.Number, BoundValueType.Number)] =
                (generator, arguments) => generator.EmitCompare(OpCodes.Cgt, arguments, true);

            result[GetOperationMethodKey(Operation.GreaterThan, BoundValueType.Number, BoundValueType.Number)] =
                (generator, arguments) => generator.EmitCompare(OpCodes.Cgt, arguments);

            result[GetOperationMethodKey(Operation.GreaterThanOrEqual, BoundValueType.Number, BoundValueType.Number)] =
                (generator, arguments) => generator.EmitCompare(OpCodes.Clt, arguments, true);

            // I don't now why, but using these is slower than using the operation
            // methods. There are methods available in the JintRuntime.Operations.cs
            // file that implement the operations below. The registrations
            // below can be uncommented to activate them.

            //result[GetOperationMethodKey(Operation.SetIndex, BoundValueType.Unknown, BoundValueType.Unknown, BoundValueType.Unknown)] =
            //    (generator, arguments) => Expression.Assign(
            //        Expression.MakeIndex(
            //            Expression.Convert(arguments[0], typeof(JsObject)),
            //            _indexByInstance,
            //            new[] { arguments[1] }
            //        ),
            //        arguments[2]
            //    );

            //result[GetOperationMethodKey(Operation.SetIndex, BoundValueType.Unknown, BoundValueType.Double, BoundValueType.Unknown)] =
            //    (generator, arguments) => Expression.Call(
            //        typeof(JintRuntime).GetMethod("Operation_SetIndex", new[] { typeof(JsObject), typeof(double), typeof(object) }),
            //        Expression.Convert(arguments[0], typeof(JsObject)),
            //        arguments[1],
            //        arguments[2]
            //    );

            //result[GetOperationMethodKey(Operation.Index, BoundValueType.Unknown, BoundValueType.Double)] =
            //    (generator, arguments) => Expression.Condition(
            //        Expression.Property(
            //            arguments[0],
            //            "IsObject"
            //        ),
            //        Expression.Call(
            //            runtime,
            //            typeof(JintRuntime).GetMethod("Operation_Index", new[] { typeof(JsObject), typeof(double) }),
            //            Expression.Convert(arguments[0], typeof(JsObject)),
            //            arguments[1]
            //        ),
            //        Expression.Call(
            //            runtime,
            //            typeof(JintRuntime).GetMethod("Operation_Index", new[] { typeof(object), typeof(object) }),
            //            arguments[0],
            //            Expression.Call(
            //                typeof(JsNumber).GetMethod("Box"),
            //                arguments[1]
            //            )
            //        )
            //    );

            //result[GetOperationMethodKey(Operation.Index, BoundValueType.Object, BoundValueType.Unknown)] =
            //    (generator, arguments) => Expression.MakeIndex(
            //        arguments[0],
            //        typeof(JsObject).GetProperty("Item", new[] { typeof(object) }),
            //        new[] { arguments[1] }
            //    );

            return result;
        }

        private BoundValueType EmitCompare(OpCode opCode, IEnumerable<BoundExpression> arguments)
        {
            return EmitCompare(opCode, arguments, false);
        }

        private BoundValueType EmitCompare(OpCode opCode, IEnumerable<BoundExpression> arguments, bool inverse)
        {
            EmitExpressions(arguments);
            IL.Emit(opCode);

            if (inverse)
                IL.EmitNot();

            return BoundValueType.Boolean;
        }

        private BoundValueType EmitNumberOperation(OpCode opCode, IEnumerable<BoundExpression> arguments)
        {
            EmitExpressions(arguments);
            IL.Emit(opCode);

            return BoundValueType.Number;
        }

        private BoundValueType EmitBitwiseOperation(OpCode opCode, IEnumerable<BoundExpression> arguments)
        {
            foreach (var argument in arguments)
            {
                if (EmitExpression(argument) == BoundValueType.Unknown)
                    IL.Emit(OpCodes.Unbox_Any, typeof(double));
                IL.Emit(OpCodes.Conv_I8);
            }

            IL.Emit(opCode);
            IL.Emit(OpCodes.Conv_R8);

            return BoundValueType.Number;
        }

        private BoundValueType EmitBitwiseShiftOperation(OpCode opCode, IList<BoundExpression> arguments)
        {
            Debug.Assert(arguments.Count == 2);

            // Push the first argument as a long.
            if (EmitExpression(arguments[0]) == BoundValueType.Unknown)
                IL.Emit(OpCodes.Unbox_Any, typeof(double));
            IL.Emit(OpCodes.Conv_I8);

            // Push the second argument as an int with an intermediate cast of
            // an ushort to trim excessive bits.
            if (EmitExpression(arguments[1]) == BoundValueType.Unknown)
                IL.Emit(OpCodes.Unbox_Any, typeof(double));
            IL.Emit(OpCodes.Conv_U2);
            IL.Emit(OpCodes.Conv_I4);

            IL.Emit(opCode);
            IL.Emit(OpCodes.Conv_R8);

            return BoundValueType.Number;
        }

        private static Func<CodeGenerator, BoundExpression[], BoundValueType> FindOperationBuilder(Operation operation, BoundValueType operand)
        {
            return FindOperationBuilder(operation, operand, BoundValueType.Unset);
        }

        private static Func<CodeGenerator, BoundExpression[], BoundValueType> FindOperationBuilder(Operation operation, BoundValueType left, BoundValueType right)
        {
            return FindOperationBuilder(operation, left, right, BoundValueType.Unset);
        }

        private static Func<CodeGenerator, BoundExpression[], BoundValueType> FindOperationBuilder(Operation operation, BoundValueType a, BoundValueType b, BoundValueType c)
        {
            Func<CodeGenerator, BoundExpression[], BoundValueType> result;
            _operationBuilders.TryGetValue(GetOperationMethodKey(operation, a, b, c), out result);
            return result;
        }
    }
}
