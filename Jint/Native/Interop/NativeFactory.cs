using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Jint.Runtime;

namespace Jint.Native.Interop
{
    internal static class NativeFactory
    {
        public static readonly object WrappingMarker = new object();

        private static readonly MethodInfo _createStruct = typeof(NativeFactory).GetMethod("CreateStruct", BindingFlags.NonPublic | BindingFlags.Static);

        public static JsFunction BuildNativeConstructor(JsGlobal global, Type type, JsObject basePrototype)
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

            // if this is a value type, define a default constructor
            if (type.IsValueType)
            {
                overloads.DefineCustomOverload(
                    new Type[0],
                    new Type[0],
                    (WrappedConstructor)Delegate.CreateDelegate(typeof(WrappedConstructor), _createStruct.MakeGenericMethod(type))
                );
            }

            // Now we should find all static members and add them as a properties

            // Members are grouped by their names

            foreach (var member in GetMethods(type, BindingFlags.Static | BindingFlags.Public))
            {
                prototype.DefineOwnProperty(member.Key, ReflectOverload(global, member.Value));
            }

            // Find and add all static properties and fields

            foreach (var info in type.GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                prototype.DefineOwnProperty(marshaller.MarshalPropertyInfo(info, prototype));
            }

            foreach (var info in type.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                prototype.DefineOwnProperty(marshaller.MarshalFieldInfo(info, prototype));
            }

            if (type.IsEnum)
            {
                string[] names = Enum.GetNames(type);
                object[] values = new object[names.Length];
                Enum.GetValues(type).CopyTo(values, 0);

                for (int i = 0; i < names.Length; i++)
                {
                    prototype.DefineOwnProperty(names[i], global.CreateObject(values[i], prototype));
                }
            }

            // find all nested types
            foreach (var info in type.GetNestedTypes(BindingFlags.Public))
            {
                prototype.DefineOwnProperty(info.Name, marshaller.MarshalClrValue(info), PropertyAttributes.DontEnum);
            }

            // Find all instance properties and fields

            var getMethods = new List<MethodInfo>();
            var setMethods = new List<MethodInfo>();
            var properties = new List<NativeDescriptor>();

            foreach (var info in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var indexerParams = info.GetIndexParameters();

                if (indexerParams.Length == 0)
                {
                    properties.Add(global.Marshaller.MarshalPropertyInfo(info, prototype));
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
                properties.Add(global.Marshaller.MarshalFieldInfo(info, prototype));
            }

            foreach (var member in GetMethods(type, BindingFlags.Instance | BindingFlags.Public))
            {
                prototype[member.Key] = ReflectOverload(global, member.Value);
            }

            prototype.SetProperty(Id.toString, ProxyHelper.BuildMethodFunction(
                global,
                typeof(object).GetMethod("ToString")
            ));

            var result = global.CreateFunction(
                type.FullName,
                new Constructor(type, overloads, propertyStoreFactory, properties).Execute,
                0,
                null,
                prototype,
                true
            );

            // HACK: When the delegate is going to be put into Value, this will
            // give a problem.

            Debug.Assert(result.Value == null);
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

        private static JsFunction ReflectOverload(JsGlobal global, List<MethodInfo> methods)
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

        private static JsFunction BuildMethodOverload(JsGlobal global, List<MethodInfo> methods)
        {
            return global.CreateFunction(
                methods[0].Name,
                new NativeMethodOverload(global, methods).Execute,
                methods[0].GetParameters().Length,
                null
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
        private static object CreateStruct<T>(JsGlobal global, JsInstance[] args) where T : struct
        {
            return new T();
        }
// ReSharper restore UnusedMember.Local

        private class Constructor
        {
            private readonly Type _reflectedType;
            private readonly NativeOverloadImpl<ConstructorInfo, WrappedConstructor> _overloads;
            private readonly Func<JsObject, IPropertyStore> _propertyStoreFactory;
            private readonly List<NativeDescriptor> _properties;

            public Constructor(Type reflectedType, NativeOverloadImpl<ConstructorInfo, WrappedConstructor> overloads, Func<JsObject, IPropertyStore> propertyStoreFactory, List<NativeDescriptor> properties)
            {
                _reflectedType = reflectedType;
                _overloads = overloads;
                _propertyStoreFactory = propertyStoreFactory;
                _properties = properties;
            }

            public JsInstance Execute(JintRuntime runtime, JsInstance @this, JsFunction callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (@this == null || @this == runtime.Global.GlobalScope)
                    throw new JintException("A constructor '" + _reflectedType.FullName + "' should be applied to the object");

                if (@this.Value != WrappingMarker)
                {
                    if (@this.Value != null)
                        throw new JintException("Can't apply the constructor '" + _reflectedType.FullName + "' to already initialized '" + @this.Value + "'");

                    @this.Value = CreateInstance(runtime.Global, arguments);
                }

                var @object = (JsObject)@this;

                if (_propertyStoreFactory != null)
                    @object.PropertyStore = _propertyStoreFactory(@object);

                SetupNativeProperties(@object);

                return @this;
            }

            private void SetupNativeProperties(JsObject target)
            {
                if (target == null)
                    throw new ArgumentException("A valid JS object is required", "target");

                foreach (var property in _properties)
                {
                    target.DefineOwnProperty(new NativeDescriptor(target, property));
                }
            }

            /// <summary>
            /// Finds a best matched constructor and uses it to create a native object instance
            /// </summary>
            private object CreateInstance(JsGlobal global, JsInstance[] parameters)
            {
                if (parameters == null)
                    parameters = JsInstance.EmptyArray;

                var implementation = _overloads.ResolveOverload(parameters, null);
                if (implementation == null)
                {
                    throw new JintException(
                        String.Format("No matching overload found {0}({1})",
                            _reflectedType.FullName,
                            String.Join(",", Array.ConvertAll(parameters, p => p.ToString()))
                        )
                    );
                }

                return implementation(global, parameters);
            }
        }
    }
}
