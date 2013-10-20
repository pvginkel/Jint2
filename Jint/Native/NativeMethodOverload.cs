using System;
using System.Collections.Generic;
using System.Text;
using Jint.Marshal;
using System.Reflection;
using Jint.Expressions;

namespace Jint.Native
{
    public class NativeMethodOverload : JsFunction
    {
        private readonly Marshaller _marshaller;
        private readonly NativeOverloadImpl<MethodInfo, JsMethodImpl> _overloads;

        // a list of methods
        private readonly LinkedList<MethodInfo> _methods = new LinkedList<MethodInfo>();

        // a list of generics
        private readonly LinkedList<MethodInfo> _generics = new LinkedList<MethodInfo>();

        public NativeMethodOverload(ICollection<MethodInfo> methods, JsObject prototype, IGlobal global)
            : base(prototype)
        {
            if (global == null)
                throw new ArgumentNullException("global");
            _marshaller = global.Marshaller;

            foreach (MethodInfo info in methods)
            {
                Name = info.Name;
                break;
            }

            foreach (var method in methods)
            {
                if (method.IsGenericMethodDefinition)
                    _generics.AddLast(method);
                else if (!method.ContainsGenericParameters)
                    _methods.AddLast(method);
            }

            _overloads = new NativeOverloadImpl<MethodInfo, JsMethodImpl>(
                _marshaller,
                new NativeOverloadImpl<MethodInfo, JsMethodImpl>.GetMembersDelegate(GetMembers),
                new NativeOverloadImpl<MethodInfo, JsMethodImpl>.WrapMemberDelegate(WrapMember)
            );
        }

        public override bool IsClr
        {
            get
            {
                return true;
            }
        }

        public override object Value
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        public override JsInstance Execute(IJintVisitor visitor, JsDictionaryObject that, JsInstance[] parameters, Type[] genericArguments)
        {
            if (_generics.Count == 0 && (genericArguments != null && genericArguments.Length > 0))
                return base.Execute(visitor, that, parameters, genericArguments);
            else
            {
                JsMethodImpl impl = _overloads.ResolveOverload(parameters, genericArguments);
                if (impl == null)
                    throw new JintException(String.Format("No matching overload found {0}<{1}>", Name, genericArguments));
                visitor.Return(impl(visitor.Global, that, parameters));
                return that;
            }
        }

        protected JsMethodImpl WrapMember(MethodInfo info)
        {
            return _marshaller.WrapMethod(info, true);
        }

        protected IEnumerable<MethodInfo> GetMembers(Type[] genericArguments, int argCount)
        {
            if (genericArguments != null && genericArguments.Length > 0)
            {

                foreach (var item in _generics)
                {
                    // try specialize generics
                    if (item.GetGenericArguments().Length == genericArguments.Length && item.GetParameters().Length == argCount)
                    {
                        MethodInfo m = null;
                        try
                        {
                            m = item.MakeGenericMethod(genericArguments);
                        }
                        catch
                        {
                        }
                        if (m != null)
                            yield return m;
                    }
                }
            }

            foreach (var item in _methods)
            {
                ParameterInfo[] parameters = item.GetParameters();
                if (parameters.Length != argCount)
                    continue;
                yield return item;
            }
        }

        public override string GetBody()
        {
            return "[native overload]";
        }
    }
}
