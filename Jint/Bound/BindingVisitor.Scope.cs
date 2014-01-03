using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Ast;
using Jint.Compiler;

namespace Jint.Bound
{
    partial class BindingVisitor
    {
        private class Scope
        {
            private readonly Closure _sourceClosure;
            private readonly Dictionary<IIdentifier, BoundArgument> _arguments = new Dictionary<IIdentifier, BoundArgument>();
            private readonly Dictionary<IIdentifier, BoundLocalBase> _locals = new Dictionary<IIdentifier, BoundLocalBase>();
            private int _lastTemporaryIndex;

            public Scope Parent { get; private set; }
            public BoundClosure Closure { get; private set; }
            public BoundTypeManager TypeManager { get; private set; }
            public bool IsArgumentsReferenced { get; set; }
            public bool IsGlobalReferenced { get; set; }
            public bool IsGlobalScopeReferenced { get; set; }
            public bool IsThisReferenced { get; set; }

            public Scope(Scope parent, BodySyntax body, IScriptBuilder scriptBuilder)
            {
                Parent = parent;
                TypeManager = new BoundTypeManager();

                if (body.Closure != null)
                {
                    _sourceClosure = body.Closure;

                    Closure = new BoundClosure(
                        FindParentClosure(),
                        body.Closure.Fields.Select(p => TypeManager.CreateType(p, BoundTypeKind.ClosureField)),
                        scriptBuilder
                    );
                }

                foreach (var variable in body.Identifiers)
                {
                    if (variable.Index.HasValue)
                    {
                        BoundClosure closure = null;
                        if (variable.Closure != null)
                            closure = GetClosure(variable.Closure);

                        _arguments.Add(variable, new BoundArgument(variable.Name, variable.Index.Value, closure));
                    }
                    else if (variable.Closure == null)
                    {
                        BoundLocalBase local;
                        if (variable.Type == IdentifierType.Global)
                            local = new BoundGlobal(variable.IsDeclared, TypeManager.CreateType(variable.Name, BoundTypeKind.Global));
                        else
                            local = new BoundLocal(variable.IsDeclared, TypeManager.CreateType(variable.Name, BoundTypeKind.Local));

                        _locals.Add(variable, local);
                    }
                }
            }

            private BoundClosure FindParentClosure()
            {
                var parent = Parent;

                while (parent != null)
                {
                    if (parent.Closure != null)
                        return parent.Closure;

                    parent = parent.Parent;
                }

                return null;
            }

            private BoundClosure GetClosure(Closure closure)
            {
                var scope = this;

                while (scope != null)
                {
                    if (scope._sourceClosure == closure)
                        return scope.Closure;

                    scope = scope.Parent;
                }

                throw new InvalidOperationException("Cannot find bound closure");
            }

            public BoundArgument GetArgument(IIdentifier variable)
            {
                if (variable.Closure != null)
                {
                    // Find the scope this argument belongs to.

                    var scope = this;

                    while (scope != null)
                    {
                        if (variable.Closure == scope._sourceClosure)
                            return scope._arguments[variable];

                        scope = scope.Parent;
                    }

                    throw new InvalidOperationException("Cannot find bound closure");
                }

                return _arguments[variable];
            }

            public BoundLocalBase GetLocal(IIdentifier identifier)
            {
                BoundLocalBase result;
                if (!_locals.TryGetValue(identifier, out result))
                {
                    Debug.Assert(identifier.Type == IdentifierType.Global);

                    result = new BoundGlobal(false, TypeManager.CreateType(identifier.Name, BoundTypeKind.Global));
                    _locals.Add(identifier, result);
                }

                return result;
            }

            public BoundClosureField GetClosureField(IIdentifier identifier)
            {
                Debug.Assert(identifier.Closure != null);

                var closure = GetClosure(identifier.Closure);

                if (identifier.Type == IdentifierType.Arguments)
                    return closure.Fields[BoundClosure.ArgumentsFieldName];

                return closure.Fields[identifier.Name];
            }

            public IEnumerable<BoundArgument> GetArguments()
            {
                return _arguments.Values;
            }

            public IEnumerable<BoundLocalBase> GetLocals()
            {
                return _locals.Values;
            }

            public IBoundWritable GetWritable(IIdentifier variable)
            {
                if (variable.Closure != null)
                    return GetClosureField(variable);
                if (variable.Index.HasValue)
                    return GetArgument(variable);
                return GetLocal(variable);
            }

            public int GetNextTemporaryIndex()
            {
                return ++_lastTemporaryIndex;
            }
        }
    }
}
