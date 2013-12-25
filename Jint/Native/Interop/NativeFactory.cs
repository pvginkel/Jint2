using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Jint.Native.Interop
{
    internal static class NativeFactory
    {
        public static readonly object WrappingMarker = new object();

        private static readonly MethodInfo _createStruct = typeof(NativeFactory).GetMethod("CreateStruct", BindingFlags.NonPublic | BindingFlags.Static);

        public static JsObject BuildNativeConstructor(JsGlobal global, Type type, JsObject basePrototype)
        {
            if (global == null)
                throw new ArgumentNullException("global");
            if (type == null)
                throw new ArgumentNullException("type");
            if (type.IsGenericType && type.ContainsGenericParameters)
                throw new InvalidOperationException("A native constructor can't be built against an open generic");

            var marshaller = global.Marshaller;

            ConstructorInfo[] constructors = null;

            if (!type.IsAbstract)
                constructors = type.GetConstructors();

            var prototype = global.CreateObject(type, basePrototype);

            var overloads = new NativeOverloadImpl<ConstructorInfo, WrappedConstructor>(
                global,
                (genericArguments, argumentCount) => GetConstructors(constructors, genericArguments, argumentCount),
                ProxyHelper.WrapConstructor
            );

            // Ff this is a value type, define a default constructor.

            if (type.IsValueType)
            {
                overloads.DefineCustomOverload(
                    Type.EmptyTypes,
                    Type.EmptyTypes,
                    (WrappedConstructor)Delegate.CreateDelegate(typeof(WrappedConstructor), _createStruct.MakeGenericMethod(type))
                );
            }

            // Now we should find all static members and add them as a properties.

            // Members are grouped by their names.

            foreach (var member in GetMethods(type, BindingFlags.Static | BindingFlags.Public))
            {
                prototype.DefineProperty(
                    member.Key,
                    ReflectOverload(global, member.Value),
                    PropertyAttributes.None
                );
            }

            // Find and add all static properties and fields.

            foreach (var info in type.GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                marshaller.MarshalPropertyInfo(info).DefineProperty(prototype);
            }

            foreach (var info in type.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (info.IsLiteral)
                {
                    prototype.DefineProperty(
                        info.Name,
                        global.CreateObject(info.GetValue(null), prototype),
                        PropertyAttributes.None
                    );
                }
                else
                {
                    marshaller.MarshalFieldInfo(info).DefineProperty(prototype);
                }
            }

            // Find all nested types.

            foreach (var info in type.GetNestedTypes(BindingFlags.Public))
            {
                prototype.DefineProperty(info.Name, marshaller.MarshalClrValue(info), PropertyAttributes.DontEnum);
            }

            // Find all instance properties and fields.

            var getMethods = new List<MethodInfo>();
            var setMethods = new List<MethodInfo>();
            var properties = new List<MarshalAccessorProperty>();

            foreach (var info in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var indexerParams = info.GetIndexParameters();

                if (indexerParams.Length == 0)
                {
                    properties.Add(global.Marshaller.MarshalPropertyInfo(info));
                }
                else if (info.Name == "Item" && indexerParams.Length == 1)
                {
                    if (info.CanRead)
                        getMethods.Add(info.GetGetMethod());
                    if (info.CanWrite)
                        setMethods.Add(info.GetSetMethod());
                }
            }

            Func<JsObject, IPropertyStore> propertyStoreFactory = null;

            if (getMethods.Count > 0 || setMethods.Count > 0)
            {
                propertyStoreFactory = p => new NativePropertyStore(p, getMethods.ToArray(), setMethods.ToArray());
            }
            else if (type.IsArray)
            {
                propertyStoreFactory = p => (IPropertyStore)Activator.CreateInstance(
                    typeof(NativeArrayPropertyStore<>).MakeGenericType(type.GetElementType()),
                    new object[] { p, marshaller }
                );
            }

            foreach (var info in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                properties.Add(global.Marshaller.MarshalFieldInfo(info));
            }

            foreach (var member in GetMethods(type, BindingFlags.Instance | BindingFlags.Public))
            {
                prototype.SetProperty(
                    member.Key,
                    ReflectOverload(global, member.Value)
                );
            }

            prototype.SetProperty(Id.toString, ProxyHelper.BuildMethodFunction(
                global,
                typeof(object).GetMethod("ToString")
            ));

            var result = global.CreateNakedFunction(
                type.FullName,
                new Constructor(type, overloads, propertyStoreFactory, properties).Execute,
                0,
                prototype,
                true
            );

            result.IsClr = true;
            result.Value = type;

            return result;
        }

        private static Dictionary<string, List<MethodInfo>> GetMethods(Type type, BindingFlags bindingFlags)
        {
            var members = new Dictionary<string, List<MethodInfo>>();

            foreach (var info in type.GetMethods(bindingFlags))
            {
                if (info.ReturnType.IsByRef)
                    continue;

                if (!members.ContainsKey(info.Name))
                    members[info.Name] = new List<MethodInfo>();

                members[info.Name].Add(info);
            }

            return members;
        }

        private static JsObject ReflectOverload(JsGlobal global, List<MethodInfo> methods)
        {
            switch (methods.Count)
            {
                case 0:
                    throw new ArgumentException("At least one method is required", "methods");

                case 1:
                    var method = methods[0];
                    if (method.ContainsGenericParameters)
                        return BuildMethodOverload(global, methods);

                    return ProxyHelper.BuildMethodFunction(global, method);

                default:
                    return BuildMethodOverload(global, methods);
            }
        }

        private static JsObject BuildMethodOverload(JsGlobal global, List<MethodInfo> methods)
        {
            return global.CreateFunction(
                methods[0].Name,
                new NativeMethodOverload(global, methods).Execute,
                methods[0].GetParameters().Length
            );
        }

        private static IEnumerable<ConstructorInfo> GetConstructors(ConstructorInfo[] constructors, Type[] genericArguments, int argumentCount)
        {
            if (constructors == null)
                return new ConstructorInfo[0];

            return Array.FindAll(
                constructors,
                constructor => constructor.GetParameters().Length == argumentCount
            );
        }

        /// <summary>
        /// A helper which conforms a ConstructorImpl signature and used as a default constructor for the value types
        /// </summary>
// ReSharper disable UnusedMember.Local
        private static object CreateStruct<T>(JsGlobal global, object[] args)
            where T : struct
        {
            return new T();
        }
// ReSharper restore UnusedMember.Local

        private class Constructor
        {
            private readonly Type _reflectedType;
            private readonly NativeOverloadImpl<ConstructorInfo, WrappedConstructor> _overloads;
            private readonly Func<JsObject, IPropertyStore> _propertyStoreFactory;
            private readonly List<MarshalAccessorProperty> _properties;

            public Constructor(Type reflectedType, NativeOverloadImpl<ConstructorInfo, WrappedConstructor> overloads, Func<JsObject, IPropertyStore> propertyStoreFactory, List<MarshalAccessorProperty> properties)
            {
                _reflectedType = reflectedType;
                _overloads = overloads;
                _propertyStoreFactory = propertyStoreFactory;
                _properties = properties;
            }

            public object Execute(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                var target = (JsObject)@this;

                if (target == runtime.Global.GlobalScope)
                    throw new JintException("A constructor '" + _reflectedType.FullName + "' should be applied to the object");

                if (target.Value != WrappingMarker)
                {
                    if (target.Value != null)
                    {
                        throw new JintException(
                            "Can't apply the constructor '" +
                            _reflectedType.FullName +
                            "' to already initialized '" +
                            target.Value + "'"
                        );
                    }

                    target.Value = CreateInstance(runtime.Global, arguments);
                }

                var @object = (JsObject)@this;

                // We let the object setup on its own property store. This will
                // initialize a DictionaryPropertyStore for us that contains the
                // objects that we need. Afterward, we let the property store
                // factory wrap that property store in a native property store.

                SetupNativeProperties(@object);

                target.IsClr = true;

                if (_propertyStoreFactory != null)
                    @object.PropertyStore = _propertyStoreFactory(@object);

                return @this;
            }

            private void SetupNativeProperties(JsObject target)
            {
                if (target == null)
                    throw new ArgumentException("A valid JS object is required", "target");

                foreach (var property in _properties)
                {
                    property.DefineProperty(target);
                }
            }

            /// <summary>
            /// Finds a best matched constructor and uses it to create a native object instance
            /// </summary>
            private object CreateInstance(JsGlobal global, object[] parameters)
            {
                if (parameters == null)
                    parameters = JsValue.EmptyArray;

                var implementation = _overloads.ResolveOverload(parameters, null);
                if (implementation == null)
                {
                    throw new JintException(
                        String.Format("No matching overload found {0}({1})",
                            _reflectedType.FullName,
                            String.Join(",", Array.ConvertAll(parameters, p => JsValue.ToString(p)))
                        )
                    );
                }

                return implementation(global, parameters);
            }
        }
    }
}
