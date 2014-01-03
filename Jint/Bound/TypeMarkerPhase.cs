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

                switch (node.Value.Kind)
                {
                    case BoundKind.NewBuiltIn:
                        _scope.SpeculateType(
                            node.Variable,
                            ((BoundNewBuiltIn)node.Value).NewBuiltInType == BoundNewBuiltInType.Array
                                ? SpeculatedType.Array
                                : SpeculatedType.Object,
                            true
                        );
                        break;

                    case BoundKind.New:
                        var getVariable = ((BoundNew)node.Value).Expression as BoundGetVariable;
                        var type = SpeculatedType.Object;

                        if (getVariable != null)
                        {
                            var @global = getVariable.Variable as BoundGlobal;
                            if (@global != null && @global.Name == "Array")
                                type = SpeculatedType.Array;
                        }

                        _scope.SpeculateType(
                            node.Variable,
                            type,
                            true
                        );
                        break;

                    case BoundKind.RegEx:
                        _scope.SpeculateType(node.Variable, SpeculatedType.Object, true);
                        break;

                    case BoundKind.Call:
                        var call = (BoundCall)node.Value;
                        getVariable = call.Method as BoundGetVariable;
                        if (getVariable != null)
                        {
                            var @global = getVariable.Variable as BoundGlobal;
                            if (@global != null && @global.Name == "Array")
                                _scope.SpeculateType(node.Variable, SpeculatedType.Array, true);
                        }
                        break;

                    case BoundKind.GetVariable:
                        getVariable = (BoundGetVariable)node.Value;
                        _scope.SpeculateType(node.Variable, getVariable.Variable);
                        break;

                    case BoundKind.ExpressionBlock:
                        var expressionBlock = (BoundExpressionBlock)node.Value;
                        _scope.SpeculateType(node.Variable, expressionBlock.Result);
                        break;

                    case BoundKind.CreateFunction:
                        _scope.SpeculateType(node.Variable, SpeculatedType.Object, true);
                        break;
                }
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

            public override void VisitGetMember(BoundGetMember node)
            {
                var getVariable = node.Expression as BoundGetVariable;
                if (getVariable != null)
                {
                    var type = SpeculatedType.Unknown;
                    bool definite = false;
                    bool ignore = false;

                    var constant = node.Index as BoundConstant;
                    if (constant != null)
                    {
                        switch (constant.Value as string)
                        {
                            case "length":
                                // Length applies equally to arrays and objects;
                                // however, we do prefer array but it's not
                                // definite yet.
                                type = SpeculatedType.Array;
                                break;

                            case "join":
                            case "pop":
                            case "push":
                            case "reverse":
                            case "shift":
                            case "unshift":
                            case "slice":
                            case "sort":
                            case "splice":
                                type = SpeculatedType.Array;
                                definite = true;
                                break;

                            case "toFixed":
                            case "toExponential":
                            case "toPrecision":
                                // Number functions.
                                ignore = true;
                                break;
                        }
                    }

                    if (!ignore)
                    {
                        if (type == SpeculatedType.Unknown)
                        {
                            type = node.Index.ValueType == BoundValueType.Number
                                ? SpeculatedType.Array
                                : SpeculatedType.Object;
                        }

                        _scope.SpeculateType(getVariable.Variable, type, definite);
                    }
                }

                base.VisitGetMember(node);
            }

            public override void VisitSetMember(BoundSetMember node)
            {
                var getVariable = node.Expression as BoundGetVariable;
                if (getVariable != null)
                {
                    var type = node.Index.ValueType == BoundValueType.Number
                        ? SpeculatedType.Array
                        : SpeculatedType.Object;

                    _scope.SpeculateType(getVariable.Variable, type, false);
                }

                base.VisitSetMember(node);
            }

            public override void VisitDeleteMember(BoundDeleteMember node)
            {
                var getVariable = node.Expression as BoundGetVariable;
                if (getVariable != null)
                {
                    var local = getVariable.Variable as BoundLocal;
                    if (local != null)
                    {
                        var type = SpeculatedType.Object;

                        if (node.Index.ValueType == BoundValueType.Number)
                            type = SpeculatedType.Array;

                        _scope.SpeculateType(
                            local,
                            type,
                            false
                        );
                    }
                }

                base.VisitDeleteMember(node);
            }

            private class Scope : IDisposable
            {
                private BoundTypeManager.TypeMarker _marker;
                private readonly Dictionary<BoundArgument, BoundVariable> _arguments;
                private readonly Dictionary<BoundMagicVariableType, IBoundType> _magicTypes;
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

                    _magicTypes = body.TypeManager.MagicTypes.ToDictionary(p => p.MagicType, p => p.Type);

                    foreach (var item in _magicTypes)
                    {
                        switch (item.Key)
                        {
                            case BoundMagicVariableType.Arguments:
                            case BoundMagicVariableType.Global:
                                _marker.SpeculateType(item.Value, SpeculatedType.Object, true);
                                _marker.MarkWrite(item.Value, BoundValueType.Object);
                                break;

                            case BoundMagicVariableType.This:
                                _marker.MarkWrite(item.Value, BoundValueType.Unknown);
                                break;
                                
                            default:
                                throw new InvalidOperationException();
                        }
                    }
                }

                public void MarkWrite(IBoundWritable writable, BoundValueType type)
                {
                    var boundType = ResolveType(writable);

                    if (boundType != null)
                        _marker.MarkWrite(boundType, type);
                }

                public void SpeculateType(IBoundReadable target, SpeculatedType type, bool definite)
                {
                    var targetType = ResolveType(target);

                    if (targetType != null)
                    {
#if TRACE_SPECULATION
                        Trace.WriteLine("Speculating " + target + " to " + type + " definite " + definite);
#endif
                        _marker.SpeculateType(targetType, type, definite);
                    }
                }

                public void SpeculateType(IBoundReadable target, IBoundReadable source)
                {
                    var sourceType = ResolveType(source);
                    var targetType = ResolveType(target);

                    if (sourceType != null && targetType != null)
                    {
#if TRACE_SPECULATION
                        Trace.WriteLine("Speculating " + target + " from " + source);
#endif
                        _marker.SpeculateType(targetType, sourceType);
                    }
                }

                private IBoundType ResolveType(IBoundReadable readable)
                {
                    var variable = readable as BoundVariable;
                    if (variable != null)
                        return variable.Type;

                    var argument = readable as BoundArgument;
                    if (argument != null)
                    {
                        variable = GetMappedArgument(argument);
                        if (variable != null)
                            return variable.Type;

                        return null;
                    }

                    var magic = readable as BoundMagicVariable;
                    if (magic != null)
                    {
                        IBoundType result;
                        if (_magicTypes.TryGetValue(magic.VariableType, out result))
                            return result;
                    }

                    return null;
                }

                private BoundVariable GetMappedArgument(BoundArgument argument)
                {
                    var scope = this;

                    while (scope != null)
                    {
                        BoundVariable result;
                        if (
                            scope._arguments != null &&
                            scope._arguments.TryGetValue(argument, out result)
                        )
                            return result;

                        scope = scope.Parent;
                    }

                    return null;
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
