﻿using System;
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
            private readonly Dictionary<Variable, BoundLocal> _locals = new Dictionary<Variable, BoundLocal>();
            private int _lastTemporaryIndex;

            public Scope Parent { get; private set; }
            public BoundClosure Closure { get; private set; }
            public BoundTypeManager TypeManager { get; private set; }

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
                        BoundClosureField closureField = null;

                        if (variable.ClosureField != null)
                        {
                            var closure = GetClosure(variable.ClosureField.Closure);
                            closureField = closure.Fields[Expressions.Closure.ArgumentsFieldName];
                        }

                        _arguments.Add(variable, new BoundArgument(variable.Name, variable.Index, closureField));
                    }
                    else if (variable.ClosureField == null)
                    {
                        _locals.Add(variable, new BoundLocal(variable.IsDeclared, TypeManager.CreateType(variable.Name, BoundTypeKind.Local)));
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
                if (variable.ClosureField != null)
                {
                    // Find the scope this argument belongs to.

                    var scope = this;

                    while (scope != null)
                    {
                        if (variable.ClosureField.Closure == scope._sourceClosure)
                            return scope._arguments[variable];

                        scope = scope.Parent;
                    }

                    throw new InvalidOperationException("Cannot find bound closure");
                }

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
