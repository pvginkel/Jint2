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
        private static readonly MethodInfo _toString = typeof(JsInstance).GetMethod("ToString");
        private static readonly MethodInfo _newNumber = typeof(JsNumberConstructor).GetMethod("New", new[] { typeof(double) });
        private static readonly MethodInfo _newBoolean = typeof(JsBooleanConstructor).GetMethod("New", new[] { typeof(bool) });
        private static readonly MethodInfo _newString = typeof(JsStringConstructor).GetMethod("New", new[] { typeof(string) });

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
                    case TypeCode.String: method = _toString; break;

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
                MethodInfo method;

                switch (Type.GetTypeCode(target.LimitType))
                {
                    case TypeCode.Double:
                        klass = _global.NumberClass;
                        method = _newNumber;
                        break;

                    case TypeCode.Boolean:
                        klass = _global.BooleanClass;
                        method = _newBoolean;
                        break;

                    case TypeCode.String:
                        klass = _global.StringClass;
                        method = _newString;
                        break;

                    default:
                        throw new NotImplementedException();
                }

                return new DynamicMetaObject(
                    Expression.Call(
                        Expression.Constant(klass),
                        method,
                        new[] { Expression.Convert(target.Expression, target.LimitType) }
                    ),
                    BindingRestrictions.GetExpressionRestriction(
                        Expression.TypeIs(
                            target.Expression,
                            target.LimitType
                        )
                    )
                );
            }

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
                "Cannot bind member \"" + Type + "\""
            );
             */
        }
    }
}
