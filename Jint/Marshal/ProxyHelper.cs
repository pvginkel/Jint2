using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Jint.Native;

namespace Jint.Marshal {
    /// <summary>
    /// A helper class for generating proxy code while marshalling methods, properties and delegates.
    /// </summary>
    internal class ProxyHelper {

        private struct MarshaledParameter {
            public readonly LocalBuilder TempLocal;
            public readonly int Index;

            public MarshaledParameter(LocalBuilder temp, int index) {
                TempLocal = temp;
                Index = index;
            }
        };

        private static ProxyHelper _default;

        private readonly Dictionary<MethodInfo, JsMethodImpl> _methodCache = new Dictionary<MethodInfo, JsMethodImpl>();
        private readonly Dictionary<ConstructorInfo, ConstructorImpl> _ctorCache = new Dictionary<ConstructorInfo, ConstructorImpl>();
        private readonly Dictionary<PropertyInfo, DynamicMethod> _propGetCache = new Dictionary<PropertyInfo, DynamicMethod>();
        private readonly Dictionary<PropertyInfo, DynamicMethod> _propSetCache = new Dictionary<PropertyInfo, DynamicMethod>();
        private readonly Dictionary<FieldInfo, DynamicMethod> _fieldSetCache = new Dictionary<FieldInfo, DynamicMethod>();
        private readonly Dictionary<FieldInfo, DynamicMethod> _fieldGetCache = new Dictionary<FieldInfo, DynamicMethod>();
        private readonly Dictionary<MethodInfo, DynamicMethod> _indexerGetCache = new Dictionary<MethodInfo, DynamicMethod>();
        private readonly Dictionary<MethodInfo, DynamicMethod> _indexerSetCache = new Dictionary<MethodInfo, DynamicMethod>();

        public static ProxyHelper Default {
            get {
                lock (typeof(ProxyHelper)) {
                    if (_default == null)
                        _default = new ProxyHelper();
                    return _default;
                }
            }
        }

        /// <summary>
        /// Converts a native method to a standard delegate.
        /// </summary>
        /// <param name="info">A method to wrap</param>
        /// <param name="passGlobal">If this paramerter is true and the first argument of the constructor
        /// is JsGlobal, a wrapper delegate will pass a Global JS object in the first parameter.</param>
        /// <returns>A wrapper delegate</returns>
        public JsMethodImpl WrapMethod(MethodInfo info, bool passGlobal) {
            if (info == null)
                throw new ArgumentNullException("info");
            if (info.ContainsGenericParameters)
                throw new InvalidOperationException("Can't wrap an unclosed generic");

            JsMethodImpl result;
            lock (_methodCache) {
                if (_methodCache.TryGetValue(info, out result))
                    return result;
            }

            LinkedList<ParameterInfo> parameters = new LinkedList<ParameterInfo>(info.GetParameters());
            LinkedList<MarshaledParameter> outParams = new LinkedList<MarshaledParameter>();

            DynamicMethod jsWrapper = new DynamicMethod("jsWrapper", typeof(JsInstance), new Type[] { typeof(JsGlobal), typeof(JsInstance), typeof(JsInstance[]) }, GetType());
            var code = jsWrapper.GetILGenerator();

            code.DeclareLocal(typeof(int)); // local #0: count of the passed arguments
            code.DeclareLocal(typeof(Marshaller));

            code.Emit(OpCodes.Ldarg_0);
            code.Emit(OpCodes.Call, typeof(JsGlobal).GetProperty("Marshaller").GetGetMethod());

            if (!info.ReturnType.Equals(typeof(void))) {
                // push the global.Marshaller object
                // for the future use
                code.Emit(OpCodes.Dup);
            }

            code.Emit(OpCodes.Stloc_1);

            if (!info.IsStatic) {
                var lblDesired = code.DefineLabel();
                var lblWrong = code.DefineLabel();

                // push the global.Marshaller object
                code.Emit(OpCodes.Ldloc_1);

                code.Emit(OpCodes.Ldarg_1); // 'that' parameter

                if (info.DeclaringType.IsValueType)
                    code.Emit(OpCodes.Call, typeof(Marshaller).GetMethod("MarshalJsValueBoxed").MakeGenericMethod(info.DeclaringType));
                else
                    code.Emit(OpCodes.Call, typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(info.DeclaringType));

                code.Emit(OpCodes.Dup); // remember converted result
                code.Emit(OpCodes.Ldnull);

                code.Emit(OpCodes.Beq, lblWrong);
                code.Emit(OpCodes.Br, lblDesired);

                code.MarkLabel(lblWrong);

                code.Emit(OpCodes.Ldstr, "The specified 'that' object is not acceptable for this method");
                code.Emit(OpCodes.Newobj, typeof(JintException).GetConstructor(new Type[] { typeof(string) }));
                code.Emit(OpCodes.Throw);

                code.MarkLabel(lblDesired);

                // everything is ok
                // we have a converted 'that' value in the stack now
                // if that is a value type, we need to unbox it

                if (info.DeclaringType.IsValueType)
                    code.Emit(OpCodes.Unbox, info.DeclaringType);
            }

            // if the first parameter is JsGlobal and passGlobal is enabled
            if (passGlobal && parameters.First != null && typeof(JsGlobal).IsAssignableFrom(parameters.First.Value.ParameterType)) {
                parameters.RemoveFirst();
                code.Emit(OpCodes.Ldarg_0);
                code.Emit(OpCodes.Isinst, typeof(JsGlobal));
            }

            // argsCount = arguments.Length
            code.Emit(OpCodes.Ldarg_2); // 'arguments' parameter
            code.Emit(OpCodes.Ldlen);

            code.Emit(OpCodes.Stloc_0);

            int i = 0;
            foreach (var parameter in parameters) {
                // push the global.Marshaller object
                code.Emit(OpCodes.Ldloc_1);

                // if ( argsCount > i )
                var lblDefaultValue = code.DefineLabel();
                var lblEnd = code.DefineLabel();

                code.Emit(OpCodes.Ldloc_0);
                code.Emit(OpCodes.Ldc_I4, i);
                code.Emit(OpCodes.Ble, lblDefaultValue);

                // push arguments[i]
                code.Emit(OpCodes.Ldarg_2);
                code.Emit(OpCodes.Ldc_I4, i);
                code.Emit(OpCodes.Ldelem, typeof(JsInstance));

                code.Emit(OpCodes.Br, lblEnd);
                code.MarkLabel(lblDefaultValue);
                // else

                // push JsUndefined.Instance
                code.Emit(OpCodes.Ldsfld, typeof(JsUndefined).GetField("Instance"));

                code.MarkLabel(lblEnd);

                // convert current parameter to a proper type
                if (parameter.ParameterType.IsByRef) {
                    Type paramType = parameter.ParameterType.GetElementType();
                    LocalBuilder tempLocal = code.DeclareLocal(paramType);
                    // marshall
                    code.Emit(
                        OpCodes.Call,
                        typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(paramType)
                    );
                    // store value in the temp variable
                    code.Emit(OpCodes.Stloc, tempLocal.LocalIndex);
                    // load a reference to the variable
                    code.Emit(OpCodes.Ldloca, tempLocal.LocalIndex);

                    if (parameter.IsOut)
                        outParams.AddLast(new MarshaledParameter(tempLocal, i));
                } else {
                    code.Emit(
                        OpCodes.Call,
                        typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(parameter.ParameterType)
                    );
                }

                i++;
            }

            // now we have an optional 'that' parameter followed by the sequence of converted arguments
            if (!info.IsStatic)
                code.Emit(OpCodes.Callvirt, info);
            else
                code.Emit(OpCodes.Call, info);

            // unmarshal out parameters
            foreach (var param in outParams) {
                // if (argcount > i)
                var lblEnd = code.DefineLabel();

                code.Emit(OpCodes.Ldloc_0);
                code.Emit(OpCodes.Ldc_I4, param.Index);
                code.Emit(OpCodes.Ble, lblEnd);

                // set arguments[i] = marshaller.MarshalClrValue(tempLocal);
                code.Emit(OpCodes.Ldarg_2);
                code.Emit(OpCodes.Ldc_I4, param.Index);

                code.Emit(OpCodes.Ldloc_1);
                code.Emit(OpCodes.Ldloc, param.TempLocal);
                code.Emit(OpCodes.Call, typeof(Marshaller).GetMethod("MarshalClrValue").MakeGenericMethod(param.TempLocal.LocalType));

                code.Emit(OpCodes.Stelem, typeof(JsInstance));

                code.MarkLabel(lblEnd);
            }

            if (!info.ReturnType.Equals(typeof(void))) {
                // convert a result into JsInstance
                code.Emit(OpCodes.Call, typeof(Marshaller).GetMethod("MarshalClrValue").MakeGenericMethod(info.ReturnType));
            } else {
                // push JsUndefined.Instance
                code.Emit(OpCodes.Ldsfld, typeof(JsUndefined).GetField("Instance"));
            }

            code.Emit(OpCodes.Ret);

            result = (JsMethodImpl)jsWrapper.CreateDelegate(typeof(JsMethodImpl));
            lock (_methodCache) {
                _methodCache[info] = result;
            }
            return result;
        }

        /// <summary>
        /// Converts a constructor to a standart delegate
        /// </summary>
        /// <param name="info">A constructor to wrap</param>
        /// <param name="passGlobal">If this paramerter is true and the first argument of the constructor
        /// is JsGlobal, a wrapper delegate will pass a Global JS object in the first parameter.</param>
        /// <returns>A wrapper delegate</returns>
        public ConstructorImpl WrapConstructor(ConstructorInfo info, bool passGlobal) {
            if (info == null)
                throw new ArgumentNullException("info");

            ConstructorImpl result;
            lock (_ctorCache) {
                if (_ctorCache.TryGetValue(info, out result))
                    return result;
            }

            LinkedList<ParameterInfo> parameters = new LinkedList<ParameterInfo>(info.GetParameters());

            DynamicMethod dm = new DynamicMethod("clrConstructor", typeof(object), new Type[] { typeof(JsGlobal), typeof(JsInstance[]) }, GetType());
            var code = dm.GetILGenerator();

            code.DeclareLocal(typeof(int)); // local #0: count of the passed arguments

            if (passGlobal && parameters.First != null && typeof(JsGlobal).IsAssignableFrom(parameters.First.Value.ParameterType)) {
                parameters.RemoveFirst();
                code.Emit(OpCodes.Ldarg_0);
                code.Emit(OpCodes.Isinst, typeof(JsGlobal));
            }

            // argsCount = arguments.Length
            code.Emit(OpCodes.Ldarg_1); // 'arguments' parameter
            code.Emit(OpCodes.Ldlen);

            code.Emit(OpCodes.Stloc_0);

            int i = 0;
            foreach (var parameter in parameters) {

                // push the global.Marshaller object
                code.Emit(OpCodes.Ldarg_0);
                code.EmitCall(OpCodes.Call, typeof(JsGlobal).GetProperty("Marshaller").GetGetMethod(), null);

                // if ( argsCount > i )
                var lblDefaultValue = code.DefineLabel();
                var lblEnd = code.DefineLabel();

                code.Emit(OpCodes.Ldloc_0);
                code.Emit(OpCodes.Ldc_I4, i);
                code.Emit(OpCodes.Ble, lblDefaultValue);

                // push arguments[i]
                code.Emit(OpCodes.Ldarg_1);
                code.Emit(OpCodes.Ldc_I4, i);
                code.Emit(OpCodes.Ldelem, typeof(JsInstance));

                code.Emit(OpCodes.Br, lblEnd);
                code.MarkLabel(lblDefaultValue);
                // else

                // push JsUndefined.Instance
                code.Emit(OpCodes.Ldsfld, typeof(JsUndefined).GetField("Instance"));

                code.MarkLabel(lblEnd);

                // convert current parameter to a proper type
                if (parameter.ParameterType.IsByRef) {
                    Type paramType = parameter.ParameterType.GetElementType();
                    LocalBuilder tempLocal = code.DeclareLocal(paramType);
                    // marshall
                    code.Emit(
                        OpCodes.Call,
                        typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(paramType)
                    );
                    // store value in the temp variable
                    code.Emit(OpCodes.Stloc, tempLocal.LocalIndex);
                    // load a reference to the variable
                    code.Emit(OpCodes.Ldloca, tempLocal.LocalIndex);
                } else {
                    code.Emit(
                        OpCodes.Call,
                        typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(parameter.ParameterType)
                    );
                }

                i++;
            }

            // now we have a sequence of converted arguments

            code.Emit(OpCodes.Newobj, info);

            if (info.DeclaringType.IsValueType)
                code.Emit(OpCodes.Box, info.DeclaringType);

            code.Emit(OpCodes.Ret);

            result = (ConstructorImpl)dm.CreateDelegate(typeof(ConstructorImpl));

            lock (_ctorCache) {
                _ctorCache[info] = result;
            }

            return result;
        }

        public JsGetter WrapGetProperty(PropertyInfo prop, Marshaller marshaller) {
            if (prop == null)
                throw new ArgumentNullException("prop");

            DynamicMethod dm;
            lock (_propGetCache) {
                _propGetCache.TryGetValue(prop, out dm);
            }

            if (dm == null) {
                dm = new DynamicMethod("dynamicPropertyGetter", typeof(JsInstance), new Type[] { typeof(Marshaller), typeof(JsDictionaryObject) }, GetType());

                MethodInfo info = prop.GetGetMethod();

                var code = dm.GetILGenerator();

                code.Emit(OpCodes.Ldarg_0);

                if (!info.IsStatic) {
                    code.Emit(OpCodes.Dup);
                    code.Emit(OpCodes.Ldarg_1);

                    if (prop.DeclaringType.IsValueType) {
                        //LocalBuilder temp = code.DeclareLocal(prop.DeclaringType);
                        code.Emit(OpCodes.Callvirt, typeof(Marshaller).GetMethod("MarshalJsValueBoxed").MakeGenericMethod(prop.DeclaringType));
                        code.Emit(OpCodes.Unbox, prop.DeclaringType);
                        //code.Emit(OpCodes.Stloc,temp);
                        //code.Emit(OpCodes.Ldloca,temp);
                    } else {
                        code.Emit(OpCodes.Callvirt, typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(prop.DeclaringType));
                    }
                    code.Emit(OpCodes.Callvirt, info);
                } else {
                    // static methods should be invoked with OpCodes.Call
                    code.Emit(OpCodes.Call, info);
                }

                code.Emit(OpCodes.Call, typeof(Marshaller).GetMethod("MarshalClrValue").MakeGenericMethod(prop.PropertyType));

                code.Emit(OpCodes.Ret);

                lock (_propGetCache) {
                    _propGetCache[prop] = dm;
                }
            }

            return (JsGetter)dm.CreateDelegate(typeof(JsGetter), marshaller);
        }

        public JsGetter WrapGetField(FieldInfo field, Marshaller marshaller) {
            if (field == null)
                throw new ArgumentNullException("field");

            DynamicMethod dm;
            lock (_fieldGetCache) {
                _fieldGetCache.TryGetValue(field, out dm);
            }
            if (dm == null) {
                dm = new DynamicMethod("dynamicFieldGetter", typeof(JsInstance), new Type[] { typeof(Marshaller), typeof(JsDictionaryObject) }, GetType());
                var code = dm.GetILGenerator();

                code.Emit(OpCodes.Ldarg_0);

                if (!field.IsStatic) {
                    code.Emit(OpCodes.Dup);
                    code.Emit(OpCodes.Ldarg_1);

                    if (field.DeclaringType.IsValueType) {
                        code.Emit(OpCodes.Callvirt, typeof(Marshaller).GetMethod("MarshalJsValueBoxed").MakeGenericMethod(field.DeclaringType));
                        code.Emit(OpCodes.Unbox, field.DeclaringType);
                    } else {
                        code.Emit(OpCodes.Callvirt, typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(field.DeclaringType));
                    }
                    code.Emit(OpCodes.Ldfld, field);
                } else {
                    code.Emit(OpCodes.Ldsfld, field);
                }

                code.Emit(OpCodes.Call, typeof(Marshaller).GetMethod("MarshalClrValue").MakeGenericMethod(field.FieldType));

                code.Emit(OpCodes.Ret);

                lock (_fieldGetCache) {
                    _fieldGetCache[field] = dm;
                }
            }

            return (JsGetter)dm.CreateDelegate(typeof(JsGetter), marshaller); ;
        }

        public JsSetter WrapSetProperty(PropertyInfo prop, Marshaller marshaller) {
            if (prop == null)
                throw new ArgumentNullException("prop");

            DynamicMethod dm;

            lock (_propSetCache) {
                _propSetCache.TryGetValue(prop, out dm);
            }
            if (dm == null) {
                dm = new DynamicMethod("dynamicPropertySetter", null, new Type[] { typeof(Marshaller), typeof(JsDictionaryObject), typeof(JsInstance) }, GetType());
                MethodInfo info = prop.GetSetMethod();

                var code = dm.GetILGenerator();

                if (!info.IsStatic) {
                    code.Emit(OpCodes.Ldarg_0);
                    code.Emit(OpCodes.Ldarg_1);

                    if (prop.DeclaringType.IsValueType) {
                        //LocalBuilder temp = code.DeclareLocal(prop.DeclaringType);
                        code.Emit(OpCodes.Call, typeof(Marshaller).GetMethod("MarshalJsValueBoxed").MakeGenericMethod(prop.DeclaringType));
                        code.Emit(OpCodes.Unbox, prop.DeclaringType);
                        //code.Emit(OpCodes.Stloc, temp);
                        //code.Emit(OpCodes.Ldloca, temp);
                    } else {
                        code.Emit(OpCodes.Callvirt, typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(prop.DeclaringType));
                    }
                }

                code.Emit(OpCodes.Ldarg_0);
                code.Emit(OpCodes.Ldarg_2);
                code.Emit(OpCodes.Callvirt, typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(prop.PropertyType));

                if (info.IsStatic)
                    code.Emit(OpCodes.Call, info);
                else
                    code.Emit(OpCodes.Callvirt, info);

                code.Emit(OpCodes.Ret);

                lock (_propSetCache) {
                    _propSetCache[prop] = dm;
                }
            }

            return (JsSetter)dm.CreateDelegate(typeof(JsSetter), marshaller);
        }

        public JsSetter WrapSetField(FieldInfo field, Marshaller marshaller) {
            if (field == null)
                throw new ArgumentNullException("field");

            DynamicMethod dm;

            lock (_fieldSetCache) {
                _fieldSetCache.TryGetValue(field, out dm);
            }

            if (dm == null) {
                dm = new DynamicMethod("dynamicPropertySetter", null, new Type[] { typeof(Marshaller), typeof(JsDictionaryObject), typeof(JsInstance) }, GetType());

                var code = dm.GetILGenerator();

                if (!field.IsStatic) {
                    code.Emit(OpCodes.Ldarg_0);
                    code.Emit(OpCodes.Ldarg_1);

                    if (field.DeclaringType.IsValueType) {
                        //LocalBuilder temp = code.DeclareLocal(prop.DeclaringType);
                        code.Emit(OpCodes.Callvirt, typeof(Marshaller).GetMethod("MarshalJsValueBoxed").MakeGenericMethod(field.DeclaringType));
                        code.Emit(OpCodes.Unbox, field.DeclaringType);
                        //code.Emit(OpCodes.Stloc, temp);
                        //code.Emit(OpCodes.Ldloca, temp);
                    } else {
                        code.Emit(OpCodes.Callvirt, typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(field.DeclaringType));
                    }
                }

                code.Emit(OpCodes.Ldarg_0);
                code.Emit(OpCodes.Ldarg_2);
                code.Emit(OpCodes.Callvirt, typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(field.FieldType));

                if (field.IsStatic)
                    code.Emit(OpCodes.Stsfld, field);
                else
                    code.Emit(OpCodes.Stfld, field);

                code.Emit(OpCodes.Ret);

                lock (_fieldSetCache) {
                    _fieldSetCache[field] = dm;
                }
            }
            return (JsSetter)dm.CreateDelegate(typeof(JsSetter), marshaller);
        }

        public JsIndexerGetter WrapIndexerGetter(MethodInfo getMethod, Marshaller marshaller) {
            if (getMethod == null)
                throw new ArgumentNullException("getMethod");

            DynamicMethod dm;

            lock (_indexerGetCache) {
                _indexerGetCache.TryGetValue(getMethod, out dm);
            }

            if (dm == null) {

                if (getMethod.GetParameters().Length != 1 || getMethod.ReturnType.Equals(typeof(void)))
                    throw new ArgumentException("Invalid getter", "getMethod");



                dm = new DynamicMethod("dynamicIndexerSetter", typeof(JsInstance), new Type[] { typeof(Marshaller), typeof(JsInstance), typeof(JsInstance) }, GetType());

                ILGenerator code = dm.GetILGenerator();

                code.Emit(OpCodes.Ldarg_0);

                if (!getMethod.IsStatic) {
                    code.Emit(OpCodes.Ldarg_0);
                    code.Emit(OpCodes.Ldarg_1);

                    if (getMethod.DeclaringType.IsValueType) {
                        //LocalBuilder temp = code.DeclareLocal(prop.DeclaringType);
                        code.Emit(OpCodes.Call, typeof(Marshaller).GetMethod("MarshalJsValueBoxed").MakeGenericMethod(getMethod.DeclaringType));
                        code.Emit(OpCodes.Unbox, getMethod.DeclaringType);
                        //code.Emit(OpCodes.Stloc, temp);
                        //code.Emit(OpCodes.Ldloca, temp);
                    } else {
                        code.Emit(OpCodes.Call, typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(getMethod.DeclaringType));
                    }
                }

                {
                    var param = getMethod.GetParameters()[0];
                    code.Emit(OpCodes.Ldarg_0);
                    code.Emit(OpCodes.Ldarg_2);

                    if (param.ParameterType.IsByRef) {
                        code.Emit(OpCodes.Call, typeof(Marshaller).GetMethod("MarshalJsValueBoxed").MakeGenericMethod(param.ParameterType));
                        code.Emit(OpCodes.Unbox, param.ParameterType);
                    } else {
                        code.Emit(OpCodes.Call, typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(param.ParameterType));
                    }
                }

                if (getMethod.IsStatic)
                    code.Emit(OpCodes.Call, getMethod);
                else
                    code.Emit(OpCodes.Callvirt, getMethod);

                code.Emit(OpCodes.Callvirt, typeof(Marshaller).GetMethod("MarshalClrValue").MakeGenericMethod(getMethod.ReturnType));

                code.Emit(OpCodes.Ret);

                lock (_indexerGetCache) {
                    _indexerGetCache[getMethod] = dm;
                }
            }

            return (JsIndexerGetter)dm.CreateDelegate(typeof(JsIndexerGetter), marshaller);
        }

        public JsIndexerSetter WrapIndexerSetter(MethodInfo setMethod, Marshaller marshaller) {
            if (setMethod == null)
                throw new ArgumentNullException("getMethod");

            DynamicMethod dm;

            lock (_indexerSetCache) {
                _indexerSetCache.TryGetValue(setMethod, out dm);
            }

            if (dm == null) {

                if (!(setMethod.GetParameters().Length == 2 && setMethod.ReturnType.Equals(typeof(void))))
                    throw new ArgumentException("Invalid getter", "getMethod");

                dm = new DynamicMethod("dynamicIndexerSetter", typeof(void), new Type[] { typeof(Marshaller), typeof(JsInstance), typeof(JsInstance), typeof(JsInstance) },GetType());

                ILGenerator code = dm.GetILGenerator();

                if (!setMethod.IsStatic) {
                    code.Emit(OpCodes.Ldarg_0);
                    code.Emit(OpCodes.Ldarg_1);

                    if (setMethod.DeclaringType.IsValueType) {
                        //LocalBuilder temp = code.DeclareLocal(prop.DeclaringType);
                        code.Emit(OpCodes.Callvirt, typeof(Marshaller).GetMethod("MarshalJsValueBoxed").MakeGenericMethod(setMethod.DeclaringType));
                        code.Emit(OpCodes.Unbox, setMethod.DeclaringType);
                        //code.Emit(OpCodes.Stloc, temp);
                        //code.Emit(OpCodes.Ldloca, temp);
                    } else {
                        code.Emit(OpCodes.Callvirt, typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(setMethod.DeclaringType));
                    }
                }

                {
                    var param = setMethod.GetParameters()[0];
                    code.Emit(OpCodes.Ldarg_0);
                    code.Emit(OpCodes.Ldarg_2);

                    if (param.ParameterType.IsByRef) {
                        code.Emit(OpCodes.Call, typeof(Marshaller).GetMethod("MarshalJsValueBoxed").MakeGenericMethod(param.ParameterType));
                        code.Emit(OpCodes.Unbox, param.ParameterType);
                    } else {
                        code.Emit(OpCodes.Call, typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(param.ParameterType));
                    }
                }

                {
                    var param = setMethod.GetParameters()[1];
                    code.Emit(OpCodes.Ldarg_0);
                    code.Emit(OpCodes.Ldarg_3);

                    if (param.ParameterType.IsByRef) {
                        code.Emit(OpCodes.Call, typeof(Marshaller).GetMethod("MarshalJsValueBoxed").MakeGenericMethod(param.ParameterType));
                        code.Emit(OpCodes.Unbox, param.ParameterType);
                    } else {
                        code.Emit(OpCodes.Call, typeof(Marshaller).GetMethod("MarshalJsValue").MakeGenericMethod(param.ParameterType));
                    }
                }

                if (setMethod.IsStatic)
                    code.Emit(OpCodes.Call, setMethod);
                else
                    code.Emit(OpCodes.Callvirt, setMethod);

                code.Emit(OpCodes.Ret);

                lock (_indexerSetCache) {
                    _indexerSetCache[setMethod] = dm;
                }
            }

            return (JsIndexerSetter)dm.CreateDelegate(typeof(JsIndexerSetter), marshaller);
        }
    }
}
