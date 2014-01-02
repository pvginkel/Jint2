using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal static class TypeMarkerPhase
    {
        public static void Perform(BoundProgram node)
        {
            Perform(node.Body);
        }

        public static void Perform(BoundFunction node)
        {
            Perform(node.Body);
        }

        private static void Perform(BoundBody body)
        {
            new Marker().Visit(body);
        }

        private class Marker : BoundTreeWalker
        {
            private Scope _scope;

            public override void VisitBody(BoundBody node)
            {
                _scope = new Scope(node, _scope);

                using (_scope)
                {
                    base.VisitBody(node);
                }

                _scope = _scope.Parent;
            }

            public override void VisitSetVariable(BoundSetVariable node)
            {
                base.VisitSetVariable(node);

                _scope.MarkWrite(node.Variable, node.Value.ValueType);
            }

            public override void VisitForEachIn(BoundForEachIn node)
            {
                _scope.MarkWrite(node.Target, BoundValueType.String);

                base.VisitForEachIn(node);
            }

            public override void VisitCatch(BoundCatch node)
            {
                _scope.MarkWrite(node.Target, BoundValueType.Unknown);

                base.VisitCatch(node);
            }

            public override void VisitCreateFunction(BoundCreateFunction node)
            {
                node.Function.Body.Accept(this);
            }

            private class Scope : IDisposable
            {
                private BoundTypeManager.TypeMarker _marker;
                private readonly Dictionary<BoundArgument, BoundVariable> _arguments;
                private bool _disposed;

                public Scope Parent { get; private set; }

                public Scope(BoundBody body, Scope parent)
                {
                    Parent = parent;

                    _marker = body.TypeManager.CreateTypeMarker();

                    foreach (var argument in body.Arguments)
                    {
                        if (argument.Closure != null)
                            MarkWrite(body.Closure.Fields[argument.Name], BoundValueType.Unknown);
                    }

                    if (body.MappedArguments != null)
                        _arguments = body.MappedArguments.ToDictionary(p => p.Argument, p => p.Mapped);

                    if (body.MappedArguments != null)
                    {
                        foreach (var mapping in body.MappedArguments)
                        {
                            MarkWrite(mapping.Mapped, BoundValueType.Unknown);
                        }
                    }

                    if (body.Closure != null)
                    {
                        var argumentsClosureField = body.Closure.Fields.SingleOrDefault(p => p.Name == BoundClosure.ArgumentsFieldName);
                        if (argumentsClosureField != null)
                            MarkWrite(argumentsClosureField, BoundValueType.Object);
                    }
                }

                public void MarkWrite(IBoundWritable writable, BoundValueType type)
                {
                    IBoundType boundType = null;

                    var variable = writable as BoundVariable;
                    if (variable != null)
                    {
                        boundType = variable.Type;
                    }
                    else if (_arguments != null)
                    {
                        var argument = writable as BoundArgument;
                        if (argument != null)
                            boundType = GetMappedArgument(argument).Type;
                    }

                    if (boundType != null)
                        _marker.MarkWrite(boundType, type);
                }

                private BoundVariable GetMappedArgument(BoundArgument argument)
                {
                    var scope = this;

                    while (scope != null)
                    {
                        BoundVariable result;
                        if (scope._arguments.TryGetValue(argument, out result))
                            return result;

                        scope = scope.Parent;
                    }

                    // Shouldn't get here.

                    throw new InvalidOperationException();
                }

                public void Dispose()
                {
                    if (!_disposed)
                    {
                        if (_marker != null)
                        {
                            _marker.Dispose();
                            _marker = null;
                        }

                        _disposed = true;
                    }
                }
            }
        }
    }
}
