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
    internal class JintConvertBinder : ConvertBinder
    {
        private readonly IGlobal _global;
        private static readonly MethodInfo _toNumber = typeof(JsInstance).GetMethod("ToNumber");
        private static readonly MethodInfo _toBoolean = typeof(JsInstance).GetMethod("ToBoolean");
        private static readonly MethodInfo _newNumber = typeof(JsNumberConstructor).GetMethod("New", new[] { typeof(double) });
        private static readonly MethodInfo _newBoolean = typeof(JsBooleanConstructor).GetMethod("New", new[] { typeof(bool) });

        public JintConvertBinder(IGlobal global, Type type, bool @explicit)
            : base(type, @explicit)
        {
            _global = global;
        }

        public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            if (typeof(JsInstance).IsAssignableFrom(target.LimitType))
            {
                MethodInfo method;

                switch (Type.GetTypeCode(Type))
                {
                    case TypeCode.Double: method = _toNumber; break;
                    case TypeCode.Boolean: method = _toBoolean; break;

                    default:
                        throw new NotImplementedException();
                }

                return new DynamicMetaObject(
                    Expression.Call(
                        target.Expression,
                        method
                    ),
                    BindingRestrictions.GetExpressionRestriction(
                        Expression.TypeIs(
                            target.Expression,
                            typeof(JsInstance)
                        )
                    )
                );
            }
            else
            {
                JsConstructor klass;
                Type type;
                MethodInfo method;

                switch (Type.GetTypeCode(target.LimitType))
                {
                    case TypeCode.Double:
                        klass = _global.NumberClass;
                        method = _newNumber;
                        type = typeof(double);
                        break;

                    case TypeCode.Boolean:
                        klass = _global.BooleanClass;
                        method = _newBoolean;
                        type = typeof(bool);
                        break;

                    default:
                        throw new NotImplementedException();
                }

                return new DynamicMetaObject(
                    Expression.Call(
                        Expression.Constant(klass),
                        method,
                        new[] { Expression.Convert(target.Expression, type) }
                    ),
                    BindingRestrictions.GetExpressionRestriction(
                        Expression.TypeIs(
                            target.Expression,
                            type
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
                "Cannot bind member \"" + Type + "\""
            );
        }
    }
}
