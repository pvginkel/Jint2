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
        public JintGetIndexBinder(CallInfo callInfo)
            : base(callInfo)
        {
        }

        public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
        {
            if (
                typeof(JsDictionaryObject).IsAssignableFrom(target.LimitType) &&
                indexes.Length == 1 &&
                typeof(JsInstance).IsAssignableFrom(indexes[0].LimitType)
            ) {
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
                "Cannot bind set index"
            );
        }
    }
}
