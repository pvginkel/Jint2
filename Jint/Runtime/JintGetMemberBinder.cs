using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Jint.Native;

namespace Jint.Runtime
{
    internal class JintGetMemberBinder : GetMemberBinder
    {
        public JintGetMemberBinder(string name)
            : base(name, false)
        {
        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            if (typeof(JsDictionaryObject).IsAssignableFrom(target.LimitType))
            {
                return new DynamicMetaObject(
                    Expression.Property(
                        Expression.Convert(target.Expression, typeof(JsDictionaryObject)),
                        "Item",
                        Expression.Constant(Name)
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
                "Cannot bind member \"" + Name + "\""
            );
        }
    }
}
