using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Jint.Native;

namespace Jint.Runtime
{
    internal class JintUnaryOperationBinder : UnaryOperationBinder
    {
        private readonly JintContext _context;

        public JintUnaryOperationBinder(JintContext context, ExpressionType operation)
            : base(operation)
        {
            _context = context;
        }

        public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            if (typeof(JsInstance).IsAssignableFrom(target.LimitType))
            {
                switch (Operation)
                {
                    case ExpressionType.Not:
                        return new DynamicMetaObject(
                            Expression.Dynamic(
                                _context.Convert(typeof(JsInstance), true),
                                typeof(JsInstance),
                                Expression.Not(
                                    Expression.Dynamic(
                                        _context.Convert(typeof(bool), true),
                                        typeof(bool),
                                        target.Expression
                                    )
                                )
                            ),
                            BindingRestrictions.GetExpressionRestriction(
                                Expression.TypeIs(
                                    target.Expression,
                                    typeof(JsInstance)
                                )
                            )
                        );
                }
            }

            throw new NotImplementedException();
        }
    }
}
