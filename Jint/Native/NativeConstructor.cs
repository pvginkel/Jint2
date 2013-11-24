using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Jint.Marshal;

namespace Jint.Native
{
    /// <summary>
    /// A constructor function that reflects a native clr type to the js runtime.
    /// </summary>
    /// <remarks>
    /// This class doesn't used to wrap open generics, since open generics can't be
    /// used to create instances they are not considered as functions (constructors).
    /// </remarks>
    public class NativeConstructor : JsConstructor
    {
        private readonly LinkedList<NativeDescriptor> _properties = new LinkedList<NativeDescriptor>();
        private readonly INativeIndexer _indexer;

        private readonly ConstructorInfo[] _constructors;
        private readonly Marshaller _marshaller;
        private readonly NativeOverloadImpl<ConstructorInfo, ConstructorImpl> _overloads;
        private readonly Type _reflectedType;

        public NativeConstructor(Type type, JsGlobal global, JsObject basePrototype)
            : base(global, null)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (type.IsGenericType && type.ContainsGenericParameters)
                throw new InvalidOperationException("A native constructor can't be built against an open generic");

            _marshaller = global.Marshaller;
            _reflectedType = type;
            Name = type.FullName;

            if (!type.IsAbstract)
                _constructors = type.GetConstructors();

            Prototype = global.ObjectClass.New(this, basePrototype);

            _overloads = new NativeOverloadImpl<ConstructorInfo, ConstructorImpl>(_marshaller, GetMembers, WrapMember);

            // if this is a value type, define a default constructor
            if (type.IsValueType)
            {
                _overloads.DefineCustomOverload(
                    new Type[0],
                    new Type[0],
                    (ConstructorImpl)Delegate.CreateDelegate(
                        typeof(ConstructorImpl),
                        typeof(NativeConstructor).GetMethod("CreateStruct", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(type)
                    )
                );
            }

            // now we should find all static members and add them as a properties

            // members are grouped by their names
            var members = new Dictionary<string, LinkedList<MethodInfo>>();

            foreach (var info in type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
            {
                if (info.ReturnType.IsByRef)
                    continue;
                if (!members.ContainsKey(info.Name))
                    members[info.Name] = new LinkedList<MethodInfo>();

                members[info.Name].AddLast(info);
            }

            // add the members to the object
            foreach (var pair in members)
                DefineOwnProperty(pair.Key, ReflectOverload(global, pair.Value));

            // find and add all static properties and fields
            foreach (var info in type.GetProperties(BindingFlags.Static | BindingFlags.Public))
                DefineOwnProperty(Global.Marshaller.MarshalPropertyInfo(info, this));

            foreach (var info in type.GetFields(BindingFlags.Static | BindingFlags.Public))
                DefineOwnProperty(Global.Marshaller.MarshalFieldInfo(info, this));

            if (type.IsEnum)
            {
                string[] names = Enum.GetNames(type);
                object[] values = new object[names.Length];
                Enum.GetValues(type).CopyTo(values, 0);

                for (int i = 0; i < names.Length; i++)
                    DefineOwnProperty(names[i], new JsObject(values[i], Prototype));

            }

            // find all nested types
            foreach (var info in type.GetNestedTypes(BindingFlags.Public))
                DefineOwnProperty(info.Name, Global.Marshaller.MarshalClrValue(info), PropertyAttributes.DontEnum);

            // find all instance properties and fields
            LinkedList<MethodInfo> getMethods = new LinkedList<MethodInfo>();
            LinkedList<MethodInfo> setMethods = new LinkedList<MethodInfo>();
            foreach (var info in type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                ParameterInfo[] indexerParams = info.GetIndexParameters();
                if (indexerParams == null || indexerParams.Length == 0)
                    _properties.AddLast(global.Marshaller.MarshalPropertyInfo(info, this));
                else if (info.Name == "Item" && indexerParams.Length == 1)
                {
                    if (info.CanRead)
                        getMethods.AddLast(info.GetGetMethod());
                    if (info.CanWrite)
                        setMethods.AddLast(info.GetSetMethod());
                }
            }

            if (getMethods.Count > 0 || setMethods.Count > 0)
            {
                MethodInfo[] getters = new MethodInfo[getMethods.Count];
                getMethods.CopyTo(getters, 0);
                MethodInfo[] setters = new MethodInfo[setMethods.Count];
                setMethods.CopyTo(setters, 0);

                _indexer = new NativeIndexer(_marshaller, getters, setters);
            }

            if (type.IsArray)
            {
                _indexer = (INativeIndexer)typeof(NativeArrayIndexer<>)
                    .MakeGenericType(type.GetElementType())
                    .GetConstructor(new[] { typeof(Marshaller) })
                    .Invoke(new object[] { _marshaller });
            }

            foreach (var info in type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
                _properties.AddLast(global.Marshaller.MarshalFieldInfo(info, this));
        }

        public void InitPrototype(JsGlobal global)
        {
            var prototype = Prototype;

            Dictionary<string, LinkedList<MethodInfo>> members = new Dictionary<string, LinkedList<MethodInfo>>();

            foreach (var info in _reflectedType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                if (info.ReturnType.IsByRef)
                    continue;

                if (!members.ContainsKey(info.Name))
                    members[info.Name] = new LinkedList<MethodInfo>();

                members[info.Name].AddLast(info);
            }

            foreach (var pair in members)
                prototype[pair.Key] = ReflectOverload(global, pair.Value);

            prototype["toString"] = new NativeMethod(_reflectedType.GetMethod("ToString", new Type[0]), global.FunctionClass.Prototype, global);
        }

        private static JsFunction ReflectOverload(JsGlobal global, ICollection<MethodInfo> methods)
        {
            if (methods.Count == 0)
                throw new ArgumentException("At least one method is required", "methods");

            if (methods.Count == 1)
            {
                foreach (MethodInfo info in methods)
                    if (info.ContainsGenericParameters)
                        return new NativeMethodOverload(methods, global.FunctionClass.Prototype, global);
                    else
                        return new NativeMethod(info, global.FunctionClass.Prototype, global);
            }
            else
            {
                return new NativeMethodOverload(methods, global.FunctionClass.Prototype, global);
            }

            // we should never come here
            throw new ApplicationException("Unexpected error");
        }

        public override bool IsClr
        {
            get { return true; }
        }

        public override object Value
        {
            get { return _reflectedType; }
            set { }
        }

        /// <summary>
        /// A helper which conforms a ConstrutorImpl signature and used as a default constructor for the value types
        /// </summary>
        /// <typeparam name="T">A value type</typeparam>
        /// <param name="global">global object</param>
        /// <param name="args">Constructor args, ignored</param>
        /// <returns>A new boxed value objec of type T</returns>
        static object CreateStruct<T>(JsGlobal global, JsInstance[] args) where T : struct
        {
            return new T();
        }

        /// <summary>
        /// Peforms a construction of a CLR instance inside the specified 
        /// </summary>
        /// <param name="global"></param>
        /// <param name="that"></param>
        /// <param name="parameters"></param>
        /// <param name="genericArguments"></param>
        /// <param name="outParameters"></param>
        /// <param name="visitor"></param>
        /// <returns></returns>
        public override JsFunctionResult Execute(JsGlobal global, JsInstance that, JsInstance[] parameters, Type[] genericArguments)
        {
            if (that == null || that is JsUndefined || that == JsNull.Instance || (that as JsGlobal) == global)
                throw new JintException("A constructor '" + _reflectedType.FullName + "' should be applied to the object");

            if (that.Value != null)
                throw new JintException("Can't apply the constructor '" + _reflectedType.FullName + "' to already initialized '" + that.Value + "'");

            that.Value = CreateInstance(global, parameters);
            SetupNativeProperties((JsObject)that);
            ((JsObject)that).Indexer = _indexer;
            return new JsFunctionResult(null, that);
        }

        /// <summary>
        /// Creates a new native object and wraps it with a JsObject.
        /// </summary>
        /// <remarks>
        /// This method is overriden to delegate a container creation to the <see cref="Wrap"/> method.
        /// </remarks>
        /// <param name="parameters">a constructor arguments</param>
        /// <param name="genericArgs">Ignored since this class represents a non-generic types</param>
        /// <param name="visitor">Execution visitor</param>
        /// <returns>A newly created js object</returns>
        public override JsObject Construct(JsInstance[] parameters, Type[] genericArgs, JsGlobal global)
        {
            return (JsObject)Wrap(CreateInstance(global, parameters));
        }

        /// <summary>
        /// Finds a best matched constructor and uses it to create a native object instance
        /// </summary>
        /// <param name="visitor">Execution visitor</param>
        /// <param name="parameters">Parameters for a constructor</param>
        /// <returns>A newly created native object</returns>
        private object CreateInstance(JsGlobal global, JsInstance[] parameters)
        {
            if (parameters == null)
                parameters = JsInstance.Empty;

            ConstructorImpl impl = _overloads.ResolveOverload(parameters, null);
            if (impl == null)
                throw new JintException(
                    String.Format("No matching overload found {0}({1})",
                        _reflectedType.FullName,
                        String.Join(",", Array.ConvertAll<JsInstance, string>(parameters, p => p.ToString()))
                    )
                );

            return impl(global, parameters);
        }

        public void SetupNativeProperties(JsObject target)
        {
            if (target == null || target == JsNull.Instance || target is JsUndefined)
                throw new ArgumentException("A valid js object is required", "target");
            foreach (var prop in _properties)
                target.DefineOwnProperty(new NativeDescriptor(target, prop));
        }

        public override JsInstance Wrap<T>(T value)
        {
            if (!_reflectedType.IsInstanceOfType(value))
                throw new JintException("Attempt to wrap '" + typeof(T).FullName + "' with '" + _reflectedType.FullName + "'");
            JsObject inst = new JsObject(Prototype);
            inst.Value = value;
            inst.Indexer = _indexer;
            SetupNativeProperties(inst);

            return inst;
        }

        protected ConstructorImpl WrapMember(ConstructorInfo info)
        {
            return _marshaller.WrapConstructor(info, true);
        }

        protected IEnumerable<ConstructorInfo> GetMembers(Type[] genericArguments, int argCount)
        {
            if (_constructors == null)
                return new ConstructorInfo[0];

            return Array.FindAll(_constructors, con => con.GetParameters().Length == argCount);
        }
    }
}
