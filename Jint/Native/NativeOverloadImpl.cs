using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Jint.Native
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
        private readonly Marshaller _marshaller;
        private readonly GetMembersDelegate _getMembers;
        private readonly WrapMemberDelegate _wrapMember;


        private class MethodMatch
        {
            public TMemberInfo Method;
            public int Weight;
            public Type[] Parameters;
        }

        public NativeOverloadImpl(Marshaller marshaller, GetMembersDelegate getMembers, WrapMemberDelegate wrapMember)
        {
            if (marshaller == null)
                throw new ArgumentNullException("marshaller");
            if (getMembers == null)
                throw new ArgumentNullException("getMembers");
            if (wrapMember == null)
                throw new ArgumentNullException("wrapMember");

            _marshaller = marshaller;
            _getMembers = getMembers;
            _wrapMember = wrapMember;
        }

        protected TMemberInfo MatchMethod(Type[] args, IEnumerable<TMemberInfo> members)
        {
            LinkedList<MethodMatch> matches = new LinkedList<MethodMatch>();

            foreach (var m in members)
                matches.AddLast(
                    new MethodMatch()
                    {
                        Method = m,
                        Parameters = Array.ConvertAll<ParameterInfo, Type>(
                            m.GetParameters(),
                            p => p.ParameterType
                        ),
                        Weight = 0
                    }
                );


            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    Type t = args[i];
                    for (var node = matches.First; node != null; )
                    {
                        var nextNode = node.Next;
                        if (t != null)
                        {
                            Type paramType = node.Value.Parameters[i];
                            if (t.Equals(paramType))
                            {
                                node.Value.Weight += 1;
                            }
                            else if (typeof(Delegate).IsAssignableFrom(paramType) && typeof(JsFunction).IsAssignableFrom(t))
                            {
                                // we can assing a js function to a delegate
                            }
                            else if (!_marshaller.IsAssignable(paramType, t))
                            {
                                matches.Remove(node);
                            }

                        }
                        else
                        {
                            // we can't assign undefined or null values to a value types
                            if (node.Value.Parameters[i].IsValueType)
                            {
                                matches.Remove(node);
                            }
                        }
                        node = nextNode;
                    }
                }
            }

            MethodMatch best = null;

            foreach (var match in matches)
                best = best == null ? match : (best.Weight < match.Weight ? match : best);

            return best == null ? null : best.Method;
        }

        protected string MakeKey(Type[] types, Type[] genericArguments)
        {
            return
                "<"
                + String.Join(
                    ",",
                    Array.ConvertAll<Type, string>(
                        genericArguments ?? new Type[0],
                        t => t == null ? "<null>" : t.FullName
                    )
                )
                + ">"
                + String.Join(
                    ",",
                    Array.ConvertAll<Type, String>(
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
            Type[] argTypes = Array.ConvertAll(args, x => _marshaller.GetInstanceType(x));
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
    }
}
