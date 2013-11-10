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
    internal class JintDeleteMemberBinder : DeleteMemberBinder
    {
        private static readonly MethodInfo _delete = typeof(JsDictionaryObject).GetMethod("Delete", new[] { typeof(string) });

        public JintDeleteMemberBinder(string name)
            : base(name, false)
        {
        }

        public override DynamicMetaObject FallbackDeleteMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            if (typeof(JsDictionaryObject).IsAssignableFrom(target.LimitType))
            {
                return new DynamicMetaObject(
                    Expression.Block(
                        Expression.Call(
                            target.Expression,
                            _delete,
                            Expression.Constant(Name)
                        ),
                        target.Expression
                    ),
                    BindingRestrictions.GetExpressionRestriction(
                        Expression.TypeIs(
                            target.Expression,
                            typeof(JsDictionaryObject)
                        )
                    )
                );
            }

            throw new InvalidOperationException();
        }
    }
}
