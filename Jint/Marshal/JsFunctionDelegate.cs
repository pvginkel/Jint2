using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Jint.Native;

namespace Jint.Marshal
{
    internal class JsFunctionDelegate
    {
// ReSharper disable NotAccessedField.Local
        private readonly IJintBackend _backend;
        private JsFunction _function;
        private JsObject _that;
// ReSharper restore NotAccessedField.Local
        private readonly Type _delegateType;

        public JsFunctionDelegate(IJintBackend backend, JsFunction function, JsObject that, Type delegateType)
        {
            if (backend == null)
                throw new ArgumentNullException("backend");
            if (function == null)
                throw new ArgumentNullException("function");
            if (delegateType == null)
                throw new ArgumentNullException("delegateType");
            if (!typeof(Delegate).IsAssignableFrom(delegateType))
                throw new ArgumentException("A delegate type is required", "delegateType");

            _backend = backend;
            _function = function;
            _delegateType = delegateType;
            _that = that;
        }

        public Delegate GetDelegate()
        {
            var invokeMethod = _delegateType.GetMethod("Invoke");

            var parameters = new List<ParameterExpression>();

            foreach (var parameter in invokeMethod.GetParameters())
            {
                parameters.Add(Expression.Parameter(
                    parameter.ParameterType,
                    parameter.Name
                ));
            }

            Expression call = Expression.Call(
                Expression.Constant(_backend),
                typeof(IJintBackend).GetMethod("ExecuteFunction"),
                Expression.Constant(_function),
                Expression.Constant(_that),
                Backend.Dlr.ExpressionVisitor.MakeArrayInit(
                    parameters.Select(p => Expression.Call(
                        Expression.Constant(_backend.Global.Marshaller),
                        typeof(Marshaller).GetMethod("MarshalClrValue").MakeGenericMethod(p.Type),
                        p
                    )),
                    typeof(JsInstance),
                    true
                ),
                Expression.Constant(null, typeof(Type[]))
            );

            if (invokeMethod.ReturnType != typeof(void))
            {
                call = Expression.Call(
                    Expression.Constant(_backend.Global.Marshaller),
                    typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(invokeMethod.ReturnType),
                    Expression.Property(
                        call,
                        typeof(JsFunctionResult).GetProperty("Result")
                    )
                );
            }

            var lambda = Expression.Lambda(
                _delegateType,
                Expression.Block(
                    invokeMethod.ReturnType,
                    call
                ),
                parameters
            );

            return lambda.Compile();
        }
    }
}
