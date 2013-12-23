using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Jint.Expressions;

namespace Jint.Bound
{
    partial class BindingVisitor
    {
        private class Scope
        {
            private readonly Closure _sourceClosure;
            private readonly Dictionary<Variable, BoundArgument> _arguments = new Dictionary<Variable, BoundArgument>();
            private readonly Dictionary<Variable, BoundLocal> _locals = new Dictionary<Variable, BoundLocal>();
            private int _lastTemporaryIndex;

            public Scope Parent { get; private set; }
            public BoundClosure Closure { get; private set; }
            public BoundTypeManager TypeManager { get; private set; }

            public Scope(Scope parent, BodySyntax body)
            {
                Parent = parent;
                TypeManager = new BoundTypeManager();

                if (body.Closure != null)
                {
                    _sourceClosure = body.Closure;
                    Closure = new BoundClosure(
                        body.ParentClosure != null ? GetClosure(body.ParentClosure) : null,
                        body.Closure.Fields.ToDictionary(p => p, p => TypeManager.CreateType(BoundTypeKind.ClosureField))
                    );
                }

                foreach (var variable in body.DeclaredVariables)
                {
                    if (variable.Index >= 0)
                        _arguments.Add(variable, new BoundArgument(variable.Name, variable.Index));
                    else if (variable.ClosureField == null)
                        _locals.Add(variable, new BoundLocal(variable.Name, TypeManager.CreateType(BoundTypeKind.Local)));
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
                return _arguments[variable];
            }

            public BoundLocal GetLocal(Variable variable)
            {
                return _locals[variable];
            }

            public BoundClosureField GetClosureField(Variable variable)
            {
                Debug.Assert(variable.ClosureField != null);

                var closure = GetClosure(variable.ClosureField.Closure);

                return closure.Fields[variable.Name];
            }

            public IEnumerable<BoundArgument> GetArguments()
            {
                return _arguments.Values;
            }

            public IEnumerable<BoundLocal> GetLocals()
            {
                return _locals.Values;
            }

            public IBoundWritable GetWritable(Variable variable)
            {
                if (variable.ClosureField != null)
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
