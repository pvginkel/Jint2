using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Jint.Native.Interop
{
    /// <summary>
    /// This class is used in the overload implementation for the NativeConstructor and NativeOverloadImplementation
    /// </summary>
    /// <typeparam name="TMemberInfo">A Member info type</typeparam>
    /// <typeparam name="TImpl">An implementation details type</typeparam>
    public class NativeOverloadImpl<TMemberInfo, TImpl>
        where TMemberInfo : MethodBase
        where TImpl : class
    {
        public delegate IEnumerable<TMemberInfo> GetMembersDelegate(Type[] genericArguments, int argCount);
        public delegate TImpl WrapMemberDelegate(TMemberInfo info);

        private readonly Dictionary<string, TImpl> _protoCache = new Dictionary<string, TImpl>();
        private readonly Dictionary<TMemberInfo, TImpl> _reflectCache = new Dictionary<TMemberInfo, TImpl>();
        private readonly JsGlobal _global;
        private readonly GetMembersDelegate _getMembers;
        private readonly WrapMemberDelegate _wrapMember;

        public NativeOverloadImpl(JsGlobal global, GetMembersDelegate getMembers, WrapMemberDelegate wrapMember)
        {
            if (global == null)
                throw new ArgumentNullException("global");
            if (getMembers == null)
                throw new ArgumentNullException("getMembers");
            if (wrapMember == null)
                throw new ArgumentNullException("wrapMember");

            _global = global;
            _getMembers = getMembers;
            _wrapMember = wrapMember;
        }

        protected TMemberInfo MatchMethod(Type[] args, IEnumerable<TMemberInfo> members)
        {
            var matches = new List<MethodMatch>();

            foreach (var member in members)
            {
                matches.Add(new MethodMatch
                {
                    Method = member,
                    Parameters = Array.ConvertAll(
                        member.GetParameters(),
                        p => p.ParameterType
                    ),
                    Weight = 0
                });
            }

            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    Type type = args[i];

                    foreach (var match in matches)
                    {
                        if (type != null)
                        {
                            Type paramType = match.Parameters[i];
                            if (type == paramType)
                            {
                                match.Weight++;
                            }
                            else if (
                                !typeof(Delegate).IsAssignableFrom(paramType) && 
                                !typeof(JsObject).IsAssignableFrom(type) &&
                                !_global.Marshaller.IsAssignable(paramType, type)
                            ) {
                                // Delegates can be assigned to a JsObject,
                                // so these don't invalidate a match.

                                match.Weight = int.MinValue;
                            }
                        }
                        else if (match.Parameters[i].IsValueType)
                        {
                            // We can't assign undefined or null values to a value types

                            match.Weight = int.MinValue;
                        }
                    }
                }
            }

            MethodMatch best = null;

            foreach (var match in matches)
            {
                best =
                    best == null
                    ? match
                    : (best.Weight < match.Weight ? match : best);
            }

            return
                best != null && best.Weight >= 0
                ? best.Method
                : null;
        }

        protected string MakeKey(Type[] types, Type[] genericArguments)
        {
            return
                "<" +
                String.Join(
                    ",",
                    Array.ConvertAll(
                        genericArguments ?? new Type[0],
                        t => t == null ? "<null>" : t.FullName
                    )
                ) +
                ">" +
                String.Join(
                    ",",
                    Array.ConvertAll(
                        types ?? new Type[0],
                        t => t == null ? "<null>" : t.FullName
                    )
                );
        }

        public void DefineCustomOverload(Type[] args, Type[] generics, TImpl impl)
        {
            _protoCache[MakeKey(args, generics)] = impl;
        }

        public TImpl ResolveOverload(JsInstance[] args, Type[] generics)
        {
            Type[] argTypes = Array.ConvertAll(args, x => _global.Marshaller.GetInstanceType(x));
            string key = MakeKey(argTypes, generics);
            TImpl method;
            if (!_protoCache.TryGetValue(key, out method))
            {
                TMemberInfo info = MatchMethod(argTypes, _getMembers(generics, args.Length));

                if (info != null && !_reflectCache.TryGetValue(info, out method))
                    _reflectCache[info] = method = _wrapMember(info);

                _protoCache[key] = method;
            }

            return method;
        }

        private class MethodMatch
        {
            public TMemberInfo Method;
            public int Weight;
            public Type[] Parameters;
        }
    }
}
