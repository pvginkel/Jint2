using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Jint.Native;

namespace Jint.Runtime
{
    public class JintSetIndexBinder : SetIndexBinder
    {
        public JintSetIndexBinder(CallInfo callInfo)
            : base(callInfo)
        {
        }

        public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            if (
                typeof(JsDictionaryObject).IsAssignableFrom(target.LimitType) &&
                indexes.Length == 1 &&
                typeof(JsInstance).IsAssignableFrom(indexes[0].LimitType)
            ) {
                return new DynamicMetaObject(
                    Expression.Assign(
                        Expression.Property(
                            Expression.Convert(target.Expression, typeof(JsDictionaryObject)),
                            "Item",
                            indexes[0].Expression
                        ),
                        value.Expression
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
        }
    }
}
