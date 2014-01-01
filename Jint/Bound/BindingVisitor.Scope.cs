using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Compiler;
using Jint.Expressions;

namespace Jint.Bound
{
    partial class BindingVisitor
    {
        private class Scope
        {
            private readonly Closure _sourceClosure;
            private readonly Dictionary<Variable, BoundArgument> _arguments = new Dictionary<Variable, BoundArgument>();
            private readonly Dictionary<Variable, BoundLocalBase> _locals = new Dictionary<Variable, BoundLocalBase>();
            private int _lastTemporaryIndex;

            public Scope Parent { get; private set; }
            public BoundClosure Closure { get; private set; }
            public BoundTypeManager TypeManager { get; private set; }
            public bool IsArgumentsReferenced { get; set; }

            public Scope(Scope parent, BodySyntax body, IScriptBuilder scriptBuilder)
            {
                Parent = parent;
                TypeManager = new BoundTypeManager();

                if (body.Closure != null)
                {
                    _sourceClosure = body.Closure;
                    Closure = new BoundClosure(
                        body.ParentClosure != null ? GetClosure(body.ParentClosure) : null,
                        body.Closure.Fields.Select(p => TypeManager.CreateType(p, BoundTypeKind.ClosureField)),
                        scriptBuilder
                    );
                }

                foreach (var variable in body.DeclaredVariables)
                {
                    if (variable.Index >= 0)
                    {
                        BoundClosure closure = null;
                        if (variable.Closure != null)
                            closure = GetClosure(variable.Closure);

                        _arguments.Add(variable, new BoundArgument(variable.Name, variable.Index, closure));
                    }
                    else if (variable.Closure == null)
                    {
                        BoundLocalBase local;
                        if (variable.Type == VariableType.Global)
                            local = new BoundGlobal(variable.IsDeclared, TypeManager.CreateType(variable.Name, BoundTypeKind.Global));
                        else
                            local = new BoundLocal(variable.IsDeclared, TypeManager.CreateType(variable.Name, BoundTypeKind.Local));

                        _locals.Add(variable, local);
                    }
                }
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

            public BoundArgument GetArgument(Variable variable)
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

            public BoundLocalBase GetLocal(Variable variable)
            {
                BoundLocalBase result;
                if (!_locals.TryGetValue(variable, out result))
                {
                    Debug.Assert(variable.Type == VariableType.Global);

                    result = new BoundGlobal(false, TypeManager.CreateType(variable.Name, BoundTypeKind.Global));
                    _locals.Add(variable, result);
                }

                return result;
            }

            public BoundClosureField GetClosureField(Variable variable)
            {
                Debug.Assert(variable.Closure != null);

                var closure = GetClosure(variable.Closure);

                if (variable.Type == VariableType.Arguments)
                    return closure.Fields[Expressions.Closure.ArgumentsFieldName];

                return closure.Fields[variable.Name];
            }

            public IEnumerable<BoundArgument> GetArguments()
            {
                return _arguments.Values;
            }

            public IEnumerable<BoundLocalBase> GetLocals()
            {
                return _locals.Values;
            }

            public IBoundWritable GetWritable(Variable variable)
            {
                if (variable.Closure != null)
                    return GetClosureField(variable);
                if (variable.Index >= 0)
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
