using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jint.Native.Interop
{
    /// <summary>
    /// A helper class for generating proxy code while marshaling methods, properties and delegates.
    /// </summary>
    internal partial class ProxyHelper
    {
        private static readonly MethodInfo _marshalJsValue = typeof(Marshaller).GetMethod("MarshalJsValue");
        private static readonly MethodInfo _marshalClr = typeof(Marshaller).GetMethod("MarshalClrValue");
        private static readonly ConcurrentDictionary<MethodInfo, JsFunction> _methodCache = new ConcurrentDictionary<MethodInfo, JsFunction>();
        private static readonly ConcurrentDictionary<PropertyInfo, JsFunction> _getPropertyCache = new ConcurrentDictionary<PropertyInfo, JsFunction>();
        private static readonly ConcurrentDictionary<PropertyInfo, JsFunction> _setPropertyCache = new ConcurrentDictionary<PropertyInfo, JsFunction>();
        private static readonly ConcurrentDictionary<FieldInfo, JsFunction> _getFieldCache = new ConcurrentDictionary<FieldInfo, JsFunction>();
        private static readonly ConcurrentDictionary<FieldInfo, JsFunction> _setFieldCache = new ConcurrentDictionary<FieldInfo, JsFunction>();
        private static readonly ConcurrentDictionary<MethodInfo, WrappedIndexerGetter> _getIndexerCache = new ConcurrentDictionary<MethodInfo, WrappedIndexerGetter>();
        private static readonly ConcurrentDictionary<MethodInfo, WrappedIndexerSetter> _setIndexerCache = new ConcurrentDictionary<MethodInfo, WrappedIndexerSetter>();
        private static readonly ConcurrentDictionary<ConstructorInfo, WrappedConstructor> _constructorCache = new ConcurrentDictionary<ConstructorInfo, WrappedConstructor>();

        public static JsObject BuildMethodFunction(JsGlobal global, MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException("method");
            if (global == null)
                throw new ArgumentNullException("global");
            if (method.ContainsGenericParameters)
                throw new InvalidOperationException("Can't wrap an unclosed generic");

            return global.CreateFunction(
                method.Name,
                WrapMethod(method),
                method.GetParameters().Length
            );
        }

        public static JsFunction WrapMethod(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            return _methodCache.GetOrAdd(method, WrapMethodCore);
        }

        private static JsFunction WrapMethodCore(MethodInfo method)
        {
            var builder = new Builder();

            builder.AddRuntimeParameter();
            builder.AddThisParameter(typeof(object));
            builder.AddParameter(typeof(JsObject), "callee");
            var argumentsParameter = builder.AddParameter(typeof(object[]), "arguments");

            var methodThis = builder.MarshalThis(method.DeclaringType, method.IsStatic);

            var arguments = new List<Expression>();

            var parameters = method.GetParameters();
            bool skipFirst = false;

            if (parameters.Length > 0 && typeof(JsGlobal).IsAssignableFrom(parameters[0].ParameterType))
            {
                skipFirst = true;
                arguments.Add(builder.Global);
            }

            var outParameters = new List<Tuple<ParameterInfo, ParameterExpression, int>>();
            int index = 0;

            foreach (var parameter in parameters)
            {
                // Skip over the global parameter.

                if (skipFirst)
                {
                    skipFirst = false;
                    continue;
                }

                // Get the argument from the array, or undefined there is none.

                Expression argument = Expression.Condition(
                    Expression.GreaterThan(
                        Expression.ArrayLength(argumentsParameter),
                        Expression.Constant(index)
                    ),
                    Expression.ArrayAccess(
                        argumentsParameter,
                        Expression.Constant(index)
                    ),
                    Expression.Field(null, typeof(JsUndefined).GetField("Instance")),
                    typeof(object)
                );

                // If this is a ref or out parameter, we need to create a local.

                if (parameter.ParameterType.IsByRef)
                {
                    var parameterType = parameter.ParameterType.GetElementType();

                    argument = builder.Marshal(argument, parameterType);

                    // Create a local to hold the value.

                    var tmpLocal = builder.AddLocal(
                        parameterType,
                        "tmp"
                    );

                    // Add the assignment to the statements.

                    builder.AddStatement(Expression.Assign(tmpLocal, argument));

                    // And use the temp local as the parameter.

                    argument = tmpLocal;

                    // Register it so that we do a write-back later on.

                    outParameters.Add(Tuple.Create(parameter, tmpLocal, index));
                }
                else
                {
                    argument = builder.Marshal(argument, parameter.ParameterType);
                }

                // Add the argument to the list.

                arguments.Add(argument);

                // Move on to the next argument.

                index++;
            }

            // Call the method.

            Expression methodCall = Expression.Call(
                methodThis,
                method,
                arguments
            );

            // Are we returning a value?

            ParameterExpression returnLocal = null;

            if (method.ReturnType != typeof(void))
            {
                returnLocal = builder.AddLocal(method.ReturnType, "return");
                
                methodCall = Expression.Assign(
                    returnLocal,
                    methodCall
                );
            }

            // Add the method call to the statements.

            builder.AddStatement(methodCall);

            // Process any out parameters.

            foreach (var outParameter in outParameters)
            {
                // Put the result back into the arguments array if the array
                // is long enough.

                builder.AddStatement(Expression.IfThen(
                    Expression.GreaterThan(
                        Expression.ArrayLength(argumentsParameter),
                        Expression.Constant(outParameter.Item3)
                    ),
                    Expression.Assign(
                        Expression.ArrayAccess(
                            argumentsParameter,
                            Expression.Constant(outParameter.Item3)
                        ),
                        Expression.Call(
                            builder.Marshaller,
                            _marshalClr.MakeGenericMethod(outParameter.Item1.ParameterType.GetElementType()),
                            outParameter.Item2
                        )
                    )
                ));
            }

            // Create the result.

            builder.AddStatement(
                returnLocal != null
                ? builder.UnMarshal(returnLocal, method.ReturnType)
                : Expression.Field(null, typeof(JsUndefined).GetField("Instance"))
            );

            return builder.Compile<JsFunction>();
        }

        public static JsObject BuildDelegateFunction(JsGlobal global, Delegate @delegate)
        {
            if (global == null)
                throw new ArgumentNullException("global");
            if (@delegate == null)
                throw new ArgumentNullException("delegate");

            return global.CreateFunction(
                @delegate.Method.Name,
                WrapDelegate(@delegate),
                @delegate.Method.GetParameters().Length
            );
        }

        private static JsFunction WrapDelegate(Delegate @delegate)
        {
            var builder = new Builder();

            builder.AddRuntimeParameter();
            builder.AddThisParameter(typeof(object));
            builder.AddParameter(typeof(JsObject), "callee");
            var argumentsParameter = builder.AddParameter(typeof(object[]), "arguments");

            var method = @delegate.Method;

            var arguments = new List<Expression>();

            var parameters = method.GetParameters();
            bool skipFirst = false;

            if (parameters.Length > 0 && typeof(JsGlobal).IsAssignableFrom(parameters[0].ParameterType))
            {
                skipFirst = true;
                arguments.Add(builder.Global);
            }

            int index = 0;

            foreach (var parameter in parameters)
            {
                // Skip over the global parameter.

                if (skipFirst)
                {
                    skipFirst = false;
                    continue;
                }

                // Get the argument from the array, or undefined there is none.

                Expression argument = Expression.Condition(
                    Expression.GreaterThan(
                        Expression.ArrayLength(argumentsParameter),
                        Expression.Constant(index)
                    ),
                    Expression.ArrayAccess(
                        argumentsParameter,
                        Expression.Constant(index)
                    ),
                    Expression.Field(null, typeof(JsUndefined).GetField("Instance")),
                    typeof(object)
                );

                // Add the argument to the list.

                arguments.Add(builder.Marshal(argument, parameter.ParameterType));

                // Move on to the next argument.

                index++;
            }

            // Call the delegate.

            builder.AddStatement(builder.UnMarshalReturn(
                Expression.Invoke(
                    Expression.Constant(@delegate),
                    arguments
                ),
                method.ReturnType
            ));

            return builder.Compile<JsFunction>();
        }

        public static JsFunction WrapGetProperty(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            return _getPropertyCache.GetOrAdd(property, WrapGetPropertyCore);
        }

        private static JsFunction WrapGetPropertyCore(PropertyInfo property)
        {
            var builder = new Builder();

            builder.AddRuntimeParameter();
            builder.AddThisParameter(typeof(object));
            builder.AddParameter(typeof(JsObject), "callee");
            builder.AddParameter(typeof(object[]), "arguments");

            var method = property.GetGetMethod();

            builder.AddStatement(builder.UnMarshal(
                Expression.Property(
                    builder.MarshalThis(method.DeclaringType, method.IsStatic),
                    property
                ),
                method.ReturnType
            ));

            return builder.Compile<JsFunction>();
        }

        public static JsFunction WrapSetProperty(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            return _setPropertyCache.GetOrAdd(property, WrapSetPropertyCore);
        }

        private static JsFunction WrapSetPropertyCore(PropertyInfo property)
        {
            var builder = new Builder();

            builder.AddRuntimeParameter();
            builder.AddThisParameter(typeof(object));
            builder.AddParameter(typeof(JsObject), "callee");
            var argumentsParameter = builder.AddParameter(typeof(object[]), "arguments");

            var method = property.GetSetMethod();

            var local = builder.AddLocal(typeof(object), "value");

            builder.AddStatement(Expression.Assign(
                local,
                Expression.ArrayAccess(
                    argumentsParameter,
                    Expression.Constant(0)
                )
            ));

            builder.AddStatement(Expression.Assign(
                Expression.Property(
                    builder.MarshalThis(method.DeclaringType, method.IsStatic),
                    property
                ),
                builder.Marshal(
                    local,
                    property.PropertyType
                )
            ));

            builder.AddStatement(local);

            return builder.Compile<JsFunction>();
        }

        public static JsFunction WrapGetField(FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            return _getFieldCache.GetOrAdd(field, WrapGetFieldCore);
        }

        private static JsFunction WrapGetFieldCore(FieldInfo field)
        {
            var builder = new Builder();

            builder.AddRuntimeParameter();
            builder.AddThisParameter(typeof(object));
            builder.AddParameter(typeof(JsObject), "callee");
            builder.AddParameter(typeof(object[]), "arguments");

            builder.AddStatement(builder.UnMarshal(
                Expression.Field(
                    builder.MarshalThis(field.DeclaringType, field.IsStatic),
                    field
                ),
                field.FieldType
            ));

            return builder.Compile<JsFunction>();
        }

        public static JsFunction WrapSetField(FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            return _setFieldCache.GetOrAdd(field, WrapSetFieldCore);
        }

        private static JsFunction WrapSetFieldCore(FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            var builder = new Builder();

            builder.AddRuntimeParameter();
            builder.AddThisParameter(typeof(object));
            builder.AddParameter(typeof(JsObject), "callee");
            var argumentsParameter = builder.AddParameter(typeof(object[]), "arguments");

            var local = builder.AddLocal(typeof(object), "value");

            builder.AddStatement(Expression.Assign(
                local,
                Expression.ArrayAccess(
                    argumentsParameter,
                    Expression.Constant(0)
                )
            ));

            // Can't assign to constants.

            if (!field.IsLiteral && !field.IsInitOnly)
            {
                builder.AddStatement(Expression.Assign(
                    Expression.Field(
                        builder.MarshalThis(field.DeclaringType, field.IsStatic),
                        field
                    ),
                    builder.Marshal(local, field.FieldType)
                ));
            }

            builder.AddStatement(local);

            return builder.Compile<JsFunction>();
        }

        public static WrappedIndexerGetter WrapIndexerGetter(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            return _getIndexerCache.GetOrAdd(method, WrapIndexerGetterCore);
        }

        private static WrappedIndexerGetter WrapIndexerGetterCore(MethodInfo method)
        {
            var builder = new Builder();

            builder.AddGlobalParameter();
            builder.AddThisParameter(typeof(JsObject));
            var indexParameter = builder.AddParameter(typeof(object), "index");

            builder.AddStatement(builder.UnMarshal(
                Expression.Call(
                    builder.MarshalThis(method.DeclaringType, method.IsStatic),
                    method,
                    builder.Marshal(indexParameter, method.GetParameters()[0].ParameterType)
                ),
                method.ReturnType
            ));

            return builder.Compile<WrappedIndexerGetter>();
        }

        public static WrappedIndexerSetter WrapIndexerSetter(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            return _setIndexerCache.GetOrAdd(method, WrapIndexerSetterCore);
        }

        private static WrappedIndexerSetter WrapIndexerSetterCore(MethodInfo method)
        {
            var builder = new Builder();

            builder.AddGlobalParameter();
            builder.AddThisParameter(typeof(JsObject));
            var indexParameter = builder.AddParameter(typeof(object), "index");
            var argument = builder.AddParameter(typeof(object), "value");

            var parameters = method.GetParameters();

            builder.AddStatement(Expression.Call(
                builder.MarshalThis(method.DeclaringType, method.IsStatic),
                method,
                builder.Marshal(indexParameter, parameters[0].ParameterType),
                builder.Marshal(argument, parameters[1].ParameterType)
            ));

            return builder.Compile<WrappedIndexerSetter>();
        }

        public static WrappedConstructor WrapConstructor(ConstructorInfo constructor)
        {
            if (constructor == null)
                throw new ArgumentNullException("constructor");

            return _constructorCache.GetOrAdd(constructor, WrapConstructorCore);
        }

        private static WrappedConstructor WrapConstructorCore(ConstructorInfo constructor)
        {
            var builder = new Builder();

            builder.AddGlobalParameter();
            var argumentsParameter = builder.AddParameter(typeof(object[]), "arguments");

            var parameters = constructor.GetParameters();
            var arguments = new List<Expression>();

            for (int i = 0; i < parameters.Length; i++)
            {
                // Get the argument from the array, or undefined there is none,
                // and marshal it.

                arguments.Add(builder.Marshal(
                    Expression.Condition(
                        Expression.GreaterThan(
                            Expression.ArrayLength(argumentsParameter),
                            Expression.Constant(i)
                        ),
                        Expression.ArrayAccess(
                            argumentsParameter,
                            Expression.Constant(i)
                        ),
                        Expression.Field(null, typeof(JsUndefined).GetField("Instance")),
                        typeof(object)
                    ),
                    parameters[i].ParameterType
                ));
            }

            // Construct the new object. This does **NOT** un-marshal the
            // result; this is done at the call site.

            builder.AddStatement(Expression.Convert(
                Expression.New(
                    constructor,
                    arguments
                ),
                typeof(object)
            ));

            return builder.Compile<WrappedConstructor>();
        }

        public static Delegate MarshalJsFunction(JintRuntime runtime, JsObject function, JsObject that, Type delegateType)
        {
            if (runtime == null)
                throw new ArgumentNullException("runtime");
            if (function == null)
                throw new ArgumentNullException("function");
            if (delegateType == null)
                throw new ArgumentNullException("delegateType");
            if (!typeof(Delegate).IsAssignableFrom(delegateType))
                throw new ArgumentException("A delegate type is required", "delegateType");

            var invokeMethod = delegateType.GetMethod("Invoke");

            var parameters = invokeMethod
                .GetParameters()
                .Select(p => Expression.Parameter(p.ParameterType, p.Name))
                .ToList();

            Expression call = Expression.Call(
                Expression.Constant(function),
                typeof(JsObject).GetMethod("Execute"),
                Expression.Constant(runtime),
                Expression.Constant(that),
                MakeArrayInit(
                    parameters.Select(p => Expression.Call(
                        Expression.Constant(runtime.Global.Marshaller),
                        typeof(Marshaller).GetMethod("MarshalClrValue").MakeGenericMethod(p.Type),
                        p
                    )),
                    typeof(object),
                    false
                )
            );

            if (invokeMethod.ReturnType != typeof(void))
            {
                call = Expression.Call(
                    Expression.Constant(runtime.Global.Marshaller),
                    typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(invokeMethod.ReturnType),
                    call
                );
            }

            var lambda = Expression.Lambda(
                delegateType,
                Expression.Block(
                    invokeMethod.ReturnType,
                    call
                ),
                parameters
            );

            return lambda.Compile();
        }

        private static Expression MakeArrayInit(IEnumerable<Expression> initializers, Type elementType, bool nullWhenEmpty)
        {
            var expressions = initializers.ToList();

            if (expressions.Count == 0)
            {
                if (nullWhenEmpty)
                    return Expression.Constant(null, elementType.MakeArrayType());

                if (elementType == typeof(object[]))
                    return Expression.Field(null, typeof(JsValue).GetField("EmptyArray"));

                return Expression.NewArrayBounds(elementType, Expression.Constant(0));
            }

            return Expression.NewArrayInit(elementType, expressions);
        }
    }
}
