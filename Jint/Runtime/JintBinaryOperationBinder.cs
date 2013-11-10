﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Jint.Native;

namespace Jint.Runtime
{
    internal class JintBinaryOperationBinder : BinaryOperationBinder
    {
        private static readonly MethodInfo _compare = typeof(JintRuntime).GetMethod("Compare");
        private static readonly MethodInfo _concat = typeof(String).GetMethod("Concat", new[] { typeof(string), typeof(string) });

        private readonly IGlobal _global;
        private readonly JintContext _context;

        public JintBinaryOperationBinder(IGlobal global, JintContext context, ExpressionType operation)
            : base(operation)
        {
            if (global == null)
                throw new ArgumentNullException("global");
            if (context == null)
                throw new ArgumentNullException("context");

            _global = global;
            _context = context;
        }

        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            if (
                typeof(JsInstance).IsAssignableFrom(target.LimitType) &&
                typeof(JsInstance).IsAssignableFrom(arg.LimitType)
            )
            {
                switch (Operation)
                {
                    case ExpressionType.Subtract:
                    case ExpressionType.Multiply:
                        return new DynamicMetaObject(
                            Expression.Convert(
                                Expression.MakeBinary(
                                    Operation,
                                    Expression.Dynamic(
                                        _context.Convert(typeof(double), true),
                                        typeof(double),
                                        target.Expression
                                    ),
                                    Expression.Dynamic(
                                        _context.Convert(typeof(double), true),
                                        typeof(double),
                                        arg.Expression
                                    )
                                ),
                                typeof(object)
                            ),
                            BindingRestrictions.GetExpressionRestriction(
                                Expression.AndAlso(
                                    Expression.TypeIs(
                                        target.Expression,
                                        typeof(JsInstance)
                                    ),
                                    Expression.TypeIs(
                                        arg.Expression,
                                        typeof(JsInstance)
                                    )
                                )
                            )
                        );

                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                        return new DynamicMetaObject(
                            Expression.Convert(
                                Expression.Call(
                                    _compare,
                                    Expression.Constant(_global),
                                    target.Expression,
                                    arg.Expression,
                                    Expression.Constant(Operation)
                                ),
                                typeof(object)
                            ),
                            BindingRestrictions.GetExpressionRestriction(
                                Expression.AndAlso(
                                    Expression.TypeIs(
                                        target.Expression,
                                        typeof(JsInstance)
                                    ),
                                    Expression.TypeIs(
                                        arg.Expression,
                                        typeof(JsInstance)
                                    )
                                )
                            )
                        );
                }
            }

            throw new NotImplementedException();

            /*
            if (typeof(JsInstance).IsAssignableFrom(target.LimitType))
            {
                return new DynamicMetaObject(
                    Expression.Assign(
                        Expression.Property(
                            target.Expression,
                            "Item",
                            Expression.Constant(Name)
                        ),
                        value.Expression
                    ),
                    BindingRestrictions.GetExpressionRestriction(
                        Expression.TypeIs(
                            target.Expression,
                            typeof(JsDictionaryObject)
                        )
                    )
                );
            }

            if (errorSuggestion != null)
                return errorSuggestion;

            return RuntimeHelpers.CreateThrow(
                target,
                null,
                BindingRestrictions.GetTypeRestriction(
                    target.Expression,
                    target.LimitType
                ),
                typeof(InvalidOperationException),
                "Cannot bind operation \"" + Operation + "\""
            );
             * */
        }
    }
}
