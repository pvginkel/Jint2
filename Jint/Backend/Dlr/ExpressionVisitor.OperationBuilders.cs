using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Jint.Expressions;
using Jint.Native;
using ValueType = Jint.Expressions.ValueType;

namespace Jint.Backend.Dlr
{
    partial class ExpressionVisitor
    {
        private static readonly MethodInfo _concat = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
        private static readonly MethodInfo _pow = typeof(Math).GetMethod("Pow", new[] { typeof(double), typeof(double) });
        private static readonly MethodInfo _substring = typeof(string).GetMethod("Substring", new[] { typeof(int), typeof(int) });
        private static readonly PropertyInfo _indexByInstance = typeof(JsDictionaryObject).GetProperty("Item", new[] { typeof(JsInstance) });
        private static readonly MethodInfo _deleteByString = typeof(JsDictionaryObject).GetMethod("Delete", new[] { typeof(string) });
        private static readonly MethodInfo _deleteByInstance = typeof(JsDictionaryObject).GetMethod("Delete", new[] { typeof(JsInstance) });
        private static readonly MethodInfo _stringEquals = typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(string) });

        private static readonly Dictionary<int, Func<Expression[], Expression>> _operationBuilders = CreateOperationBuilders();

        private static Dictionary<int, Func<Expression[], Expression>> CreateOperationBuilders()
        {
            var result = new Dictionary<int, Func<Expression[], Expression>>();

            result[GetOperationMethodKey(SyntaxExpressionType.Add, ValueType.String, ValueType.String)] =
                p => Expression.Call(
                    _concat,
                    p
                );

            result[GetOperationMethodKey(SyntaxExpressionType.Add, ValueType.Double, ValueType.Double)] =
                p => Expression.Add(p[0], p[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.Subtract, ValueType.Double, ValueType.Double)] =
                p => Expression.Subtract(p[0], p[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.BitwiseAnd, ValueType.Double, ValueType.Double)] =
                p => Expression.Convert(
                    Expression.And(
                        Expression.Convert(p[0], typeof(long)),
                        Expression.Convert(p[1], typeof(long))
                    ),
                    typeof(double)
                );

            result[GetOperationMethodKey(SyntaxExpressionType.BitwiseExclusiveOr, ValueType.Double, ValueType.Double)] =
                p => Expression.Convert(
                        Expression.ExclusiveOr(
                        Expression.Convert(p[0], typeof(long)),
                        Expression.Convert(p[1], typeof(long))
                    ),
                    typeof(double)
                );

            result[GetOperationMethodKey(SyntaxExpressionType.BitwiseNot, ValueType.Double)] =
                p => Expression.Subtract(
                    Expression.Subtract(
                        Expression.Constant(0d),
                        p[0]
                    ),
                    Expression.Constant(1d)
                );

            result[GetOperationMethodKey(SyntaxExpressionType.BitwiseOr, ValueType.Double, ValueType.Double)] =
                p => Expression.Convert(
                        Expression.Or(
                        Expression.Convert(p[0], typeof(long)),
                        Expression.Convert(p[1], typeof(long))
                    ),
                    typeof(double)
                );

            result[GetOperationMethodKey(SyntaxExpressionType.LeftShift, ValueType.Double, ValueType.Double)] =
                p => Expression.Convert(
                    Expression.LeftShift(
                        Expression.Convert(p[0], typeof(long)),
                        Expression.Convert(Expression.Convert(p[1], typeof(ushort)), typeof(int))
                    ),
                    typeof(double)
                );

            result[GetOperationMethodKey(SyntaxExpressionType.RightShift, ValueType.Double, ValueType.Double)] =
                p => Expression.Convert(
                    Expression.RightShift(
                        Expression.Convert(p[0], typeof(long)),
                        Expression.Convert(Expression.Convert(p[1], typeof(ushort)), typeof(int))
                    ),
                    typeof(double)
                );

            result[GetOperationMethodKey(SyntaxExpressionType.UnsignedRightShift, ValueType.Double, ValueType.Double)] =
                p => Expression.Convert(
                    Expression.RightShift(
                        Expression.Convert(p[0], typeof(long)),
                        Expression.Convert(Expression.Convert(p[1], typeof(ushort)), typeof(int))
                    ),
                    typeof(double)
                );

            result[GetOperationMethodKey(SyntaxExpressionType.Multiply, ValueType.Double, ValueType.Double)] =
                p => Expression.Multiply(p[0], p[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.Negate, ValueType.Double)] =
                p => Expression.Negate(p[0]);

            result[GetOperationMethodKey(SyntaxExpressionType.Not, ValueType.Boolean)] =
                p => Expression.Not(p[0]);

            result[GetOperationMethodKey(SyntaxExpressionType.Power, ValueType.Double, ValueType.Double)] =
                p => Expression.Call(
                    _pow,
                    p
                );

            result[GetOperationMethodKey(SyntaxExpressionType.TypeOf, ValueType.String)] =
                p => Expression.Constant(JsInstance.TypeString);

            result[GetOperationMethodKey(SyntaxExpressionType.TypeOf, ValueType.Double)] =
                p => Expression.Constant(JsInstance.TypeNumber);

            result[GetOperationMethodKey(SyntaxExpressionType.TypeOf, ValueType.Boolean)] =
                p => Expression.Constant(JsInstance.TypeBoolean);

            result[GetOperationMethodKey(SyntaxExpressionType.UnaryPlus, ValueType.Double)] =
                p => p[0];

            result[GetOperationMethodKey(SyntaxExpressionType.Index, ValueType.String, ValueType.Double)] =
                p => Expression.Call(
                    p[0],
                    _substring,
                    Expression.Convert(p[1], typeof(int)),
                    Expression.Constant(1)
                );

            result[GetOperationMethodKey(SyntaxExpressionType.SetIndex, ValueType.Unknown, ValueType.Unknown, ValueType.Unknown)] =
                p => Expression.Assign(
                    Expression.MakeIndex(
                        Expression.Convert(p[0], typeof(JsDictionaryObject)),
                        _indexByInstance,
                        new[] { p[1] }
                    ),
                    p[2]
                );

            result[GetOperationMethodKey(SyntaxExpressionType.Delete, ValueType.Unknown, ValueType.String)] =
                p => Expression.Call(
                    Expression.Convert(p[0], typeof(JsDictionaryObject)),
                    _deleteByString,
                    p[1]
                );

            result[GetOperationMethodKey(SyntaxExpressionType.Delete, ValueType.Unknown, ValueType.Unknown)] =
                p => Expression.Call(
                    Expression.Convert(p[0], typeof(JsDictionaryObject)),
                    _deleteByInstance,
                    p[1]
                );

            result[GetOperationMethodKey(SyntaxExpressionType.Equal, ValueType.Boolean, ValueType.Boolean)] =
                p => Expression.Equal(p[0], p[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.Equal, ValueType.Double, ValueType.Double)] =
                p => Expression.Equal(p[0], p[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.Equal, ValueType.String, ValueType.String)] =
                p => Expression.Call(_stringEquals, p);

            result[GetOperationMethodKey(SyntaxExpressionType.Same, ValueType.Boolean, ValueType.Boolean)] =
                p => Expression.Equal(p[0], p[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.Same, ValueType.Double, ValueType.Double)] =
                p => Expression.Equal(p[0], p[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.Same, ValueType.String, ValueType.String)] =
                p => Expression.Call(_stringEquals, p);

            result[GetOperationMethodKey(SyntaxExpressionType.NotEqual, ValueType.Boolean, ValueType.Boolean)] =
                p => Expression.NotEqual(p[0], p[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.NotEqual, ValueType.Double, ValueType.Double)] =
                p => Expression.NotEqual(p[0], p[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.NotEqual, ValueType.String, ValueType.String)] =
                p => Expression.Not(Expression.Call(_stringEquals, p));

            result[GetOperationMethodKey(SyntaxExpressionType.NotSame, ValueType.Boolean, ValueType.Boolean)] =
                p => Expression.NotEqual(p[0], p[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.NotSame, ValueType.Double, ValueType.Double)] =
                p => Expression.NotEqual(p[0], p[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.NotSame, ValueType.String, ValueType.String)] =
                p => Expression.Not(Expression.Call(_stringEquals, p));

            result[GetOperationMethodKey(SyntaxExpressionType.LessThan, ValueType.Double, ValueType.Double)] =
                p => Expression.LessThan(p[0], p[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.LessThanOrEqual, ValueType.Double, ValueType.Double)] =
                p => Expression.LessThanOrEqual(p[0], p[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.GreaterThan, ValueType.Double, ValueType.Double)] =
                p => Expression.GreaterThan(p[0], p[1]);

            result[GetOperationMethodKey(SyntaxExpressionType.GreaterThanOrEqual, ValueType.Double, ValueType.Double)] =
                p => Expression.GreaterThanOrEqual(p[0], p[1]);

            return result;
        }

        private static Func<Expression[], Expression> FindOperationBuilder(SyntaxExpressionType operation, ValueType operand)
        {
            return FindOperationBuilder(operation, operand, ValueType.Unset);
        }

        private static Func<Expression[], Expression> FindOperationBuilder(SyntaxExpressionType operation, ValueType left, ValueType right)
        {
            return FindOperationBuilder(operation, left, right, ValueType.Unset);
        }

        private static Func<Expression[], Expression> FindOperationBuilder(SyntaxExpressionType operation, ValueType a, ValueType b, ValueType c)
        {
            Func<Expression[], Expression> result;
            _operationBuilders.TryGetValue(GetOperationMethodKey(operation, a, b, c), out result);
            return result;
        }
    }
}
