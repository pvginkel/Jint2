using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Text;

namespace Jint.Native.Interop
{
    partial class ProxyHelper
    {
        private class Builder
        {
            private readonly List<ParameterExpression> _parameters = new List<ParameterExpression>();
            private readonly List<ParameterExpression> _locals = new List<ParameterExpression>();
            private readonly List<Expression> _statements = new List<Expression>();
            private ParameterExpression _global;
            private ParameterExpression _marshaller;
            private ParameterExpression _runtime;
            private ParameterExpression _this;

            public ParameterExpression Global
            {
                get
                {
                    Debug.Assert(_global != null);

                    return _global;
                }
            }

            public ParameterExpression Marshaller
            {
                get
                {
                    if (_marshaller == null)
                    {
                        _marshaller = AddLocal(typeof(Marshaller), "marshaller");

                        _statements.Add(Expression.Assign(
                            Marshaller,
                            Expression.Property(
                                Global,
                                "Marshaller"
                            )
                        ));
                    }

                    return _marshaller;
                }
            }

            public Builder()
            {
                _statements = new List<Expression>();
                _parameters = new List<ParameterExpression>();
                _locals = new List<ParameterExpression>();
            }

            public void AddRuntimeParameter()
            {
                Debug.Assert(_global == null);
                _runtime = AddParameter(typeof(JintRuntime), "runtime");

                // We now have the runtime available, so we can create
                // the global local.

                _global = AddLocal(typeof(JsGlobal), "global");

                _statements.Add(Expression.Assign(
                    _global,
                    Expression.Property(
                        _runtime,
                        "Global"
                    )
                ));

                AddApplyPermissionSet();
            }

            public void AddGlobalParameter()
            {
                Debug.Assert(_global == null);
                Debug.Assert(_runtime == null);
                _global = AddParameter(typeof(JsGlobal), "global");

                AddApplyPermissionSet();
            }

            private void AddApplyPermissionSet()
            {
                // Apply the permission set now. We need the Global initialized
                // to be able to set the permission set.

                _statements.Add(Expression.Call(
                    Expression.Property(
                        Expression.Property(
                            _global,
                            "Engine"
                        ),
                        "PermissionSet"
                    ),
                    typeof(PermissionSet).GetMethod("PermitOnly")
                ));
            }

            public void AddThisParameter(Type type)
            {
                _this = AddParameter(type, "@this");
            }

            public ParameterExpression AddParameter(Type type, string name)
            {
                var result = Expression.Parameter(type, name);
                _parameters.Add(result);
                return result;
            }

            public Expression MarshalThis(Type declaringType, bool isStatic)
            {
                if (isStatic)
                    return null;

                if (declaringType.IsValueType)
                {
                    Expression expression = _this;
                    if (expression.Type == typeof(JsBox))
                    {
                        expression = Expression.Call(
                            expression,
                            typeof(JsBox).GetMethod("ToInstance")
                        );
                    }

                    return Expression.Unbox(
                        Expression.Property(
                            expression,
                            "Value"
                        ),
                        declaringType
                    );
                }

                var local = Expression.Parameter(declaringType, "marshaledThis");

                return Expression.Block(
                    declaringType,
                    new[] { local },
                    Expression.Assign(
                        local,
                        Marshal(_this, declaringType)
                    ),
                    Expression.IfThen(
                        Expression.Equal(local, Expression.Constant(null)),
                        Expression.Throw(
                            Expression.New(
                                typeof(JintException).GetConstructor(new[] { typeof(string) }),
                                Expression.Constant("The specified 'that' object is not acceptable for this method")
                            )
                        )
                    ),
                    local
                );
            }

            public T Compile<T>()
            {
                // Check that Global was used anywhere. Otherwise we won't
                // have the permission set applied.

                Debug.Assert(_global != null);

                // Apply the permissions. We don't add the application here,
                // because that is done as soon as we get the Global available.

                var body = Expression.TryFinally(
                    Expression.Block(
                        _locals,
                        _statements
                    ),
                    Expression.Call(
                        typeof(CodeAccessPermission).GetMethod("RevertPermitOnly")
                    )
                );

                // Wrap the whole thing to apply permissions.

                var lambda = Expression.Lambda<T>(
                    body,
                    _parameters
                );

                return lambda.Compile();
            }

            public Expression Marshal(Expression expression, Type type)
            {
                if (typeof(JsInstance).IsAssignableFrom(expression.Type))
                {
                    expression = Expression.Call(
                        typeof(JsBox).GetMethod("FromInstance"),
                        expression
                    );
                }

                return Expression.Call(
                    Marshaller,
                    _marshalJsValue.MakeGenericMethod(type),
                    expression
                );
            }

            public Expression UnMarshal(Expression expression, Type type)
            {
                return Expression.Call(
                    Marshaller,
                    _marshalClr.MakeGenericMethod(type),
                    expression
                );
            }

            public ParameterExpression AddLocal(Type type, string name)
            {
                var result = Expression.Parameter(type, name);
                _locals.Add(result);
                return result;
            }

            public void AddStatement(Expression expression)
            {
                _statements.Add(expression);
            }

            public Expression UnMarshalReturn(Expression expression, Type type)
            {
                if (type == typeof(void))
                {
                    return Expression.Block(
                        expression,
                        Expression.Field(null, typeof(JsBox).GetField("Undefined"))
                    );
                }

                return UnMarshal(expression, type);
            }
        }
    }
}
