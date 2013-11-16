using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Jint.Native;

namespace Jint.Runtime
{
    public class JintGetIndexBinder : GetIndexBinder
    {
        private readonly JintContext _context;

        internal JintGetIndexBinder(JintContext context, CallInfo callInfo)
            : base(callInfo)
        {
            _context = context;
        }

        public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
        {
            if (
                indexes.Length == 1 &&
                typeof(JsInstance).IsAssignableFrom(indexes[0].LimitType)
            )
            {
                if (
                    typeof(JsString).IsAssignableFrom(target.LimitType) &&
                    typeof(JsNumber).IsAssignableFrom(indexes[0].LimitType)
                ) {
                    return new DynamicMetaObject(
                        Expression.Dynamic(
                            _context.Convert(typeof(JsInstance), true),
                            typeof(JsInstance),
                            Expression.Call(
                                Expression.Convert(
                                    Expression.Property(
                                        Expression.Convert(target.Expression, typeof(JsString)),
                                        typeof(JsString).GetProperty("Value")
                                    ),
                                    typeof(string)
                                ),
                                typeof(string).GetMethod("Substring", new[] { typeof(int), typeof(int) }),
                                new Expression[]
                                {
                                    Expression.Convert(
                                        Expression.Dynamic(
                                            _context.Convert(typeof(double), true),
                                            typeof(double),
                                            indexes[0].Expression
                                        ),
                                        typeof(int)
                                    ),
                                    Expression.Constant(1)
                                }
                            )
                        ),
                        BindingRestrictions.GetExpressionRestriction(
                            Expression.TypeIs(
                                target.Expression,
                                typeof(JsString)
                            )
                        ).Merge(
                            BindingRestrictions.Combine(indexes)
                        )
                    );
                }
                else if (typeof(JsDictionaryObject).IsAssignableFrom(target.LimitType))
                {
                    return new DynamicMetaObject(
                        Expression.Property(
                            Expression.Convert(target.Expression, typeof(JsDictionaryObject)),
                            "Item",
                            indexes[0].Expression
                        ),
                        BindingRestrictions.GetExpressionRestriction(
                            Expression.TypeIs(
                                target.Expression,
                                typeof(JsDictionaryObject)
                            )
                        ).Merge(
                            BindingRestrictions.Combine(indexes)
                        )
                    );
                }
            }

            throw new InvalidOperationException();
            /*
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
                            "Cannot bind get index"
                        );
            */
        }
    }
}
