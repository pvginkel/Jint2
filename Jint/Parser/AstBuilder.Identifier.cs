using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Ast;
using Jint.Native;

namespace Jint.Parser
{
    partial class AstBuilder
    {
        private class Identifier : IIdentifier
        {
            public string Name
            {
                get { return ResolvedIdentifier.Name; }
            }

            public int? Index
            {
                get { return ResolvedIdentifier.Index; }
            }

            public IdentifierType Type
            {
                get { return ResolvedIdentifier.Type; }
            }

            public Closure Closure
            {
                get { return ResolvedIdentifier.Closure; }
            }

            public WithScope WithScope
            {
                get { return ResolvedIdentifier.WithScope; }
            }

            public IIdentifier Fallback
            {
                get { return ResolvedIdentifier.Fallback; }
            }

            public bool IsDeclared
            {
                get { return ResolvedIdentifier.IsDeclared; }
            }

            public ResolvedIdentifier ResolvedIdentifier { get; set; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj))
                    return true;

                var resolved = obj as ResolvedIdentifier;
                if (resolved != null)
                    return ResolvedIdentifier == resolved;

                var identifier = obj as Identifier;
                if (identifier != null)
                    return ResolvedIdentifier == identifier.ResolvedIdentifier;

                return false;
            }

            public override int GetHashCode()
            {
                return ResolvedIdentifier.GetHashCode();
            }

            public override string ToString()
            {
                return String.Format(
                    "Name={0}, Index={1}, Type={2}, Closure={{{3}}}, IsDeclared={4}",
                    Name,
                    Index,
                    Type,
                    Closure,
                    IsDeclared
                );
            }
        }

        private class ScopedIdentifier : IIdentifier
        {
            public WithScope WithScope { get; private set; }

            public IIdentifier Fallback { get; private set; }

            public string Name
            {
                get { throw new InvalidOperationException(); }
            }

            public int? Index
            {
                get { throw new InvalidOperationException(); }
            }

            public IdentifierType Type
            {
                get { return IdentifierType.Scoped; }
            }

            public Closure Closure
            {
                get { throw new InvalidOperationException(); }
            }

            public bool IsDeclared
            {
                get { throw new InvalidOperationException(); }
            }

            public ScopedIdentifier(WithScope withScope, IIdentifier fallback)
            {
                WithScope = withScope;
                Fallback = fallback;
            }

            public override string ToString()
            {
                return String.Format(
                    "WithScope={{{0}}}, Fallback={{{1}}}",
                    WithScope,
                    Fallback
                );
            }
        }

        private class ResolvedIdentifier : IIdentifier
        {
            public static readonly ResolvedIdentifier This = new ResolvedIdentifier(JsNames.This, null, IdentifierType.This, true);
            public static readonly ResolvedIdentifier Null = new ResolvedIdentifier(JsNames.Null, null, IdentifierType.Null, true);
            public static readonly ResolvedIdentifier Undefined = new ResolvedIdentifier(JsNames.Undefined, null, IdentifierType.Undefined, true);
            public static readonly ResolvedIdentifier Arguments = new ResolvedIdentifier(JsNames.Arguments, null, IdentifierType.Arguments, true);

            public string Name { get; private set; }
            public int? Index { get; private set; }
            public IdentifierType Type { get; private set; }
            public Closure Closure { get; set; }
            public bool IsDeclared { get; private set; }

            public IIdentifier Fallback
            {
                get { return null; }
            }

            public WithScope WithScope
            {
                get { return null; }
            }

            public ResolvedIdentifier(string name, int? index, IdentifierType type, bool isDeclared)
            {
                Name = name;
                Index = index;
                Type = type;
                IsDeclared = isDeclared;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj))
                    return true;

                var identifier = obj as Identifier;
                if (identifier != null)
                    return this == identifier.ResolvedIdentifier;

                return false;
            }

            public override int GetHashCode()
            {
                // ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
                return base.GetHashCode();
                // ReSharper restore BaseObjectGetHashCodeCallInGetHashCode
            }

            public override string ToString()
            {
                return String.Format(
                    "Name={0}, Index={1}, Type={2}, Closure={{{3}}}, IsDeclared={4}",
                    Name,
                    Index,
                    Type,
                    Closure,
                    IsDeclared
                );
            }
        }
    }
}
