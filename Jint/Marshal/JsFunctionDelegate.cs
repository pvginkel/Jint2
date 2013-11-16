using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Jint.Backend.Dlr;
using Jint.Native;
using Jint.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Jint.Runtime;

namespace Jint.Marshal
{
    internal class JsFunctionDelegate
    {
        private Delegate _impl;
// ReSharper disable NotAccessedField.Local
        private readonly IJintBackend _backend;
        private readonly JintContext _context;
        private JsFunction _function;
        private JsDictionaryObject _that;
// ReSharper restore NotAccessedField.Local
        private readonly Type _delegateType;

        public JsFunctionDelegate(IJintBackend backend, JintContext context, JsFunction function, JsDictionaryObject that, Type delegateType)
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
            _context = context;
            _function = function;
            _delegateType = delegateType;
            _that = that;
        }

        public Delegate GetDelegate()
        {
            if (_impl != null)
                return _impl;

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
                call = Expression.Dynamic(
                    _context.Convert(invokeMethod.ReturnType, true),
                    invokeMethod.ReturnType,
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
/*
            _impl = _backend.ExecuteFunction(
                _function,
                _that,
                _param

            MethodInfo method = _delegateType.GetMethod("Invoke");
            ParameterInfo[] parameters = method.GetParameters();
            Type[] delegateParameters = new Type[parameters.Length + 1];

            for (int i = 1; i <= parameters.Length; i++)
                delegateParameters[i] = parameters[i - 1].ParameterType;
            delegateParameters[0] = typeof(JsFunctionDelegate);

            DynamicMethod dm = new DynamicMethod(
                "DelegateWrapper",
                method.ReturnType,
                delegateParameters,
                typeof(JsFunctionDelegate)
            );

            ILGenerator code = dm.GetILGenerator();

            // arg_0 - this
            // arg_1 ... arg_n - delegate parameters
            // local_0 parameters
            // local_1 marshaller

            code.DeclareLocal(typeof(JsInstance[]));
            code.DeclareLocal(typeof(Marshaller));

            // parameters = new JsInstance[...];
            code.Emit(OpCodes.Ldc_I4, parameters.Length);
            code.Emit(OpCodes.Newarr, typeof(JsInstance));
            code.Emit(OpCodes.Stloc_0);

            // load a marshller
            code.Emit(OpCodes.Ldarg_0);
            code.Emit(OpCodes.Ldfld, typeof(JsFunctionDelegate).GetField("_marshaller", BindingFlags.NonPublic | BindingFlags.Instance));
            code.Emit(OpCodes.Stloc_1);

            //code.EmitWriteLine("pre args");

            for (int i = 1; i <= parameters.Length; i++)
            {
                ParameterInfo param = parameters[i - 1];
                Type paramType = param.ParameterType;

                code.Emit(OpCodes.Ldloc_0);
                code.Emit(OpCodes.Ldc_I4, i - 1);

                // marshal arg
                code.Emit(OpCodes.Ldloc_1);
                code.Emit(OpCodes.Ldarg, i);

                // if parameter is passed by reference
                if (paramType.IsByRef)
                {
                    paramType = paramType.GetElementType();

                    if (param.IsOut && !param.IsIn)
                    {
                        code.Emit(OpCodes.Ldarg, i);
                        code.Emit(OpCodes.Initobj);
                    }

                    if (paramType.IsValueType)
                        code.Emit(OpCodes.Ldobj, paramType);
                    else
                        code.Emit(OpCodes.Ldind_Ref);
                }

                code.Emit(
                    OpCodes.Call,
                    typeof(Marshaller)
                        .GetMethod("MarshalClrValue")
                        .MakeGenericMethod(paramType)
                );
                // save arg

                code.Emit(OpCodes.Stelem, typeof(JsInstance));
            }

            // _visitor.ExecuteFunction(_function,_that,arguments)

            code.Emit(OpCodes.Ldarg_0);
            code.Emit(OpCodes.Ldfld, typeof(JsFunctionDelegate).GetField("_visitor", BindingFlags.NonPublic | BindingFlags.Instance));

            code.Emit(OpCodes.Ldarg_0);
            code.Emit(OpCodes.Ldfld, typeof(JsFunctionDelegate).GetField("_function", BindingFlags.NonPublic | BindingFlags.Instance));

            code.Emit(OpCodes.Ldarg_0);
            code.Emit(OpCodes.Ldfld, typeof(JsFunctionDelegate).GetField("_that", BindingFlags.NonPublic | BindingFlags.Instance));

            code.Emit(OpCodes.Ldloc_0); //params

            code.Emit(OpCodes.Callvirt, typeof(IJintVisitor).GetMethod("ExecuteFunction"));


            // foreach out parameter, marshal it back
            for (int i = 1; i <= parameters.Length; i++)
            {
                ParameterInfo param = parameters[i - 1];
                Type paramType = param.ParameterType.GetElementType();
                if (param.IsOut)
                {
                    code.Emit(OpCodes.Ldarg, i);

                    code.Emit(OpCodes.Ldloc_1);

                    code.Emit(OpCodes.Ldloc_0);
                    code.Emit(OpCodes.Ldc_I4, i - 1);
                    code.Emit(OpCodes.Ldelem, typeof(JsInstance));

                    code.Emit(OpCodes.Call, typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(paramType));

                    if (paramType.IsValueType)
                        code.Emit(OpCodes.Stobj, paramType);
                    else
                        code.Emit(OpCodes.Stind_Ref);
                }
            }

            // return marshaller.MarshalJsValue<method.ReturnType>(_visitor.Returned)
            if (!method.ReturnType.Equals(typeof(void)))
            {
                code.Emit(OpCodes.Ldloc_1);
                code.Emit(OpCodes.Ldarg_0);
                code.Emit(OpCodes.Ldfld, typeof(JsFunctionDelegate).GetField("_visitor", BindingFlags.NonPublic | BindingFlags.Instance));
                code.Emit(OpCodes.Call, typeof(IJintVisitor).GetProperty("Returned").GetGetMethod());
                code.Emit(OpCodes.Call, typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(method.ReturnType));
            }

            code.Emit(OpCodes.Ret);

            return _impl = dm.CreateDelegate(_delegateType, this);
*/
        }
    }
}
