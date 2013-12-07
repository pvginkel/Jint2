using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Jint.Expressions;
using Jint.Native;
using ValueType = Jint.Expressions.ValueType;

namespace Jint.Compiler
{
    partial class ExpressionVisitor
    {
        private static readonly MethodInfo _concat = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
        private static readonly MethodInfo _pow = typeof(Math).GetMethod("Pow", new[] { typeof(double), typeof(double) });
        private static readonly MethodInfo _substring = typeof(string).GetMethod("Substring", new[] { typeof(int), typeof(int) });
        private static readonly PropertyInfo _indexByInstance = typeof(JsObject).GetProperty("Item", new[] { typeof(JsBox) });
        private static readonly MethodInfo _deleteByString = typeof(JsObject).GetMethod("Delete", new[] { typeof(string) });
        private static readonly MethodInfo _deleteByInstance = typeof(JsObject).GetMethod("Delete", new[] { typeof(JsBox) });
        private static readonly MethodInfo _stringEquals = typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(string) });

        private static readonly Dictionary<int, Func<ParameterExpression, Expression[], Expression>> _operationBuilders = CreateOperationBuilders();

        private static Dictionary<int, Func<ParameterExpression, Expression[], Expression>> CreateOperationBuilders()
        {
            var result = new Dictionary<int, Func<ParameterExpression, Expression[], Expression>>();

            result[GetOperationMethodKey(SyntaxExpressionType.Add, ValueType.String, ValueType.String)] =
                (runtime, arguments) => Expression.Call(
                    _concat,
                    arguments
                );

            result[GetOperationMethodKey(SyntaxExpressionType.Add, ValueType.Double, ValueType.Double)] =
                (runtime, arguments) => Expression.Add(arguments[0], arguments[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.Subtract, ValueType.Double, ValueType.Double)] =
                (runtime, arguments) => Expression.Subtract(arguments[0], arguments[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.BitwiseAnd, ValueType.Double, ValueType.Double)] =
                (runtime, arguments) => Expression.Convert(
                    Expression.And(
                        Expression.Convert(arguments[0], typeof(long)),
                        Expression.Convert(arguments[1], typeof(long))
                    ),
                    typeof(double)
                );

            result[GetOperationMethodKey(SyntaxExpressionType.BitwiseExclusiveOr, ValueType.Double, ValueType.Double)] =
                (runtime, arguments) => Expression.Convert(
                    Expression.ExclusiveOr(
                        Expression.Convert(arguments[0], typeof(long)),
                        Expression.Convert(arguments[1], typeof(long))
                    ),
                    typeof(double)
                );

            result[GetOperationMethodKey(SyntaxExpressionType.BitwiseOr, ValueType.Double, ValueType.Double)] =
                (runtime, arguments) => Expression.Convert(
                    Expression.Or(
                        Expression.Convert(arguments[0], typeof(long)),
                        Expression.Convert(arguments[1], typeof(long))
                    ),
                    typeof(double)
                );

            result[GetOperationMethodKey(SyntaxExpressionType.LeftShift, ValueType.Double, ValueType.Double)] =
                (runtime, arguments) => Expression.Convert(
                    Expression.LeftShift(
                        Expression.Convert(arguments[0], typeof(long)),
                        Expression.Convert(Expression.Convert(arguments[1], typeof(ushort)), typeof(int))
                    ),
                    typeof(double)
                );

            result[GetOperationMethodKey(SyntaxExpressionType.RightShift, ValueType.Double, ValueType.Double)] =
                (runtime, arguments) => Expression.Convert(
                    Expression.RightShift(
                        Expression.Convert(arguments[0], typeof(long)),
                        Expression.Convert(Expression.Convert(arguments[1], typeof(ushort)), typeof(int))
                    ),
                    typeof(double)
                );

            result[GetOperationMethodKey(SyntaxExpressionType.UnsignedRightShift, ValueType.Double, ValueType.Double)] =
                (runtime, arguments) => Expression.Convert(
                    Expression.RightShift(
                        Expression.Convert(arguments[0], typeof(long)),
                        Expression.Convert(Expression.Convert(arguments[1], typeof(ushort)), typeof(int))
                    ),
                    typeof(double)
                );

            result[GetOperationMethodKey(SyntaxExpressionType.Multiply, ValueType.Double, ValueType.Double)] =
                (runtime, arguments) => Expression.Multiply(arguments[0], arguments[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.Negate, ValueType.Double)] =
                (runtime, arguments) => Expression.Negate(arguments[0]);

            result[GetOperationMethodKey(SyntaxExpressionType.Not, ValueType.Boolean)] =
                (runtime, arguments) => Expression.Not(arguments[0]);

            result[GetOperationMethodKey(SyntaxExpressionType.Power, ValueType.Double, ValueType.Double)] =
                (runtime, arguments) => Expression.Call(
                    _pow,
                    arguments
                );

            result[GetOperationMethodKey(SyntaxExpressionType.TypeOf, ValueType.String)] =
                (runtime, arguments) => Expression.Constant(JsNames.TypeString);

            result[GetOperationMethodKey(SyntaxExpressionType.TypeOf, ValueType.Double)] =
                (runtime, arguments) => Expression.Constant(JsNames.TypeNumber);

            result[GetOperationMethodKey(SyntaxExpressionType.TypeOf, ValueType.Boolean)] =
                (runtime, arguments) => Expression.Constant(JsNames.TypeBoolean);

            result[GetOperationMethodKey(SyntaxExpressionType.UnaryPlus, ValueType.Double)] =
                (runtime, arguments) => arguments[0];

            result[GetOperationMethodKey(SyntaxExpressionType.Index, ValueType.String, ValueType.Double)] =
                (runtime, arguments) => Expression.Call(
                    arguments[0],
                    _substring,
                    Expression.Convert(arguments[1], typeof(int)),
                    Expression.Constant(1)
                );

            result[GetOperationMethodKey(SyntaxExpressionType.Delete, ValueType.Unknown, ValueType.String)] =
                (runtime, arguments) => Expression.Call(
                    Expression.Convert(arguments[0], typeof(JsObject)),
                    _deleteByString,
                    arguments[1]
                );

            result[GetOperationMethodKey(SyntaxExpressionType.Delete, ValueType.Unknown, ValueType.Unknown)] =
                (runtime, arguments) => Expression.Call(
                    Expression.Convert(arguments[0], typeof(JsObject)),
                    _deleteByInstance,
                    arguments[1]
                );

            result[GetOperationMethodKey(SyntaxExpressionType.Equal, ValueType.Boolean, ValueType.Boolean)] =
                (runtime, arguments) => Expression.Equal(arguments[0], arguments[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.Equal, ValueType.Double, ValueType.Double)] =
                (runtime, arguments) => Expression.Equal(arguments[0], arguments[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.Equal, ValueType.String, ValueType.String)] =
                (runtime, arguments) => Expression.Call(_stringEquals, arguments);

            result[GetOperationMethodKey(SyntaxExpressionType.Same, ValueType.Boolean, ValueType.Boolean)] =
                (runtime, arguments) => Expression.Equal(arguments[0], arguments[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.Same, ValueType.Double, ValueType.Double)] =
                (runtime, arguments) => Expression.Equal(arguments[0], arguments[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.Same, ValueType.String, ValueType.String)] =
                (runtime, arguments) => Expression.Call(_stringEquals, arguments);

            result[GetOperationMethodKey(SyntaxExpressionType.NotEqual, ValueType.Boolean, ValueType.Boolean)] =
                (runtime, arguments) => Expression.NotEqual(arguments[0], arguments[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.NotEqual, ValueType.Double, ValueType.Double)] =
                (runtime, arguments) => Expression.NotEqual(arguments[0], arguments[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.NotEqual, ValueType.String, ValueType.String)] =
                (runtime, arguments) => Expression.Not(Expression.Call(_stringEquals, arguments));

            result[GetOperationMethodKey(SyntaxExpressionType.NotSame, ValueType.Boolean, ValueType.Boolean)] =
                (runtime, arguments) => Expression.NotEqual(arguments[0], arguments[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.NotSame, ValueType.Double, ValueType.Double)] =
                (runtime, arguments) => Expression.NotEqual(arguments[0], arguments[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.NotSame, ValueType.String, ValueType.String)] =
                (runtime, arguments) => Expression.Not(Expression.Call(_stringEquals, arguments));

            result[GetOperationMethodKey(SyntaxExpressionType.LessThan, ValueType.Double, ValueType.Double)] =
                (runtime, arguments) => Expression.LessThan(arguments[0], arguments[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.LessThanOrEqual, ValueType.Double, ValueType.Double)] =
                (runtime, arguments) => Expression.LessThanOrEqual(arguments[0], arguments[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.GreaterThan, ValueType.Double, ValueType.Double)] =
                (runtime, arguments) => Expression.GreaterThan(arguments[0], arguments[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.GreaterThanOrEqual, ValueType.Double, ValueType.Double)] =
                (runtime, arguments) => Expression.GreaterThanOrEqual(arguments[0], arguments[1]);

            // I don't now why, but using these is slower than using the operation
            // methods. There are methods available in the JintRuntime.Operations.cs
            // file that implement the operations below. The registrations
            // below can be uncommented to activate them.

            //result[GetOperationMethodKey(SyntaxExpressionType.SetIndex, ValueType.Unknown, ValueType.Unknown, ValueType.Unknown)] =
            //    (runtime, arguments) => Expression.Assign(
            //        Expression.MakeIndex(
            //            Expression.Convert(arguments[0], typeof(JsObject)),
            //            _indexByInstance,
            //            new[] { arguments[1] }
            //        ),
            //        arguments[2]
            //    );

            //result[GetOperationMethodKey(SyntaxExpressionType.SetIndex, ValueType.Unknown, ValueType.Double, ValueType.Unknown)] =
            //    (runtime, arguments) => Expression.Call(
            //        typeof(JintRuntime).GetMethod("Operation_SetIndex", new[] { typeof(JsObject), typeof(double), typeof(JsBox) }),
            //        Expression.Convert(arguments[0], typeof(JsObject)),
            //        arguments[1],
            //        arguments[2]
            //    );

            //result[GetOperationMethodKey(SyntaxExpressionType.Index, ValueType.Unknown, ValueType.Double)] =
            //    (runtime, arguments) => Expression.Condition(
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
            //            typeof(JintRuntime).GetMethod("Operation_Index", new[] { typeof(JsBox), typeof(JsBox) }),
            //            arguments[0],
            //            Expression.Call(
            //                typeof(JsNumber).GetMethod("Box"),
            //                arguments[1]
            //            )
            //        )
            //    );

            //result[GetOperationMethodKey(SyntaxExpressionType.Index, ValueType.Object, ValueType.Unknown)] =
            //    (runtime, arguments) => Expression.MakeIndex(
            //        arguments[0],
            //        typeof(JsObject).GetProperty("Item", new[] { typeof(JsBox) }),
            //        new[] { arguments[1] }
            //    );

            return result;
        }

        private static Func<ParameterExpression, Expression[], Expression> FindOperationBuilder(SyntaxExpressionType operation, ValueType operand)
        {
            return FindOperationBuilder(operation, operand, ValueType.Unset);
        }

        private static Func<ParameterExpression, Expression[], Expression> FindOperationBuilder(SyntaxExpressionType operation, ValueType left, ValueType right)
        {
            return FindOperationBuilder(operation, left, right, ValueType.Unset);
        }

        private static Func<ParameterExpression, Expression[], Expression> FindOperationBuilder(SyntaxExpressionType operation, ValueType a, ValueType b, ValueType c)
        {
            Func<ParameterExpression, Expression[], Expression> result;
            _operationBuilders.TryGetValue(GetOperationMethodKey(operation, a, b, c), out result);
            return result;
        }
    }
}
