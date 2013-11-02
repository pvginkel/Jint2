using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Jint.Native;

namespace Jint.Runtime
{
    internal class JintSetMemberBinder : SetMemberBinder
    {
        public JintSetMemberBinder(string name)
            : base(name, false)
        {
        }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            if (typeof(JsDictionaryObject).IsAssignableFrom(target.LimitType))
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
                "Cannot bind member \"" + Name + "\""
            );
        }
    }
}
