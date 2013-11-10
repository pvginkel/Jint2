using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Jint.Native;

namespace Jint.Runtime
{
    internal class JintDeleteIndexBinder : DeleteIndexBinder
    {
        private static readonly MethodInfo _toString = typeof(JsInstance).GetMethod("ToString");
        private static readonly MethodInfo _delete = typeof(JsDictionaryObject).GetMethod("Delete", new[] { typeof(JsInstance) });

        public JintDeleteIndexBinder(CallInfo callInfo)
            : base(callInfo)
        {
        }

        public override DynamicMetaObject FallbackDeleteIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
        {
            if (
                typeof(JsDictionaryObject).IsAssignableFrom(target.LimitType) &&
                indexes.Length == 1 &&
                typeof(JsInstance).IsAssignableFrom(indexes[0].LimitType)
            )
            {
                return new DynamicMetaObject(
                    Expression.Block(
                        Expression.Call(
                            target.Expression,
                            _delete,
                            indexes[0].Expression
                        ),
                        target.Expression
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

            throw new InvalidOperationException();
        }
    }
}
