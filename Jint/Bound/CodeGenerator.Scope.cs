using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Jint.Native;
using Jint.Support;

namespace Jint.Bound
{
    partial class CodeGenerator
    {
        private class Scope
        {
            private readonly Dictionary<IBoundType, LocalBuilder> _locals = new Dictionary<IBoundType, LocalBuilder>();
            private LocalBuilder _globalLocal;
            private LocalBuilder _globalScopeLocal;
            private readonly bool _isFunction;

            public ILBuilder IL { get; private set; }
            public BoundClosure Closure { get; private set; }
            public LocalBuilder ClosureLocal { get; private set; }
            public Scope Parent { get; private set; }
            public Dictionary<Type, LocalBuilder> ClosureLocals { get; private set; }
            public Stack<NamedLabel> BreakTargets { get; private set; }
            public Stack<NamedLabel> ContinueTargets { get; private set; }
            public BoundVariable ArgumentsVariable { get; private set; }

            public Scope(ILBuilder il, bool isFunction, BoundClosure closure, BoundVariable argumentsVariable, Scope parent)
            {
                IL = il;
                _isFunction = isFunction;
                Closure = closure;
                ArgumentsVariable = argumentsVariable;
                Parent = parent;

                ClosureLocals = new Dictionary<Type, LocalBuilder>();
                BreakTargets = new Stack<NamedLabel>();
                ContinueTargets = new Stack<NamedLabel>();

                // TODO: Is this the right thing to do?

                if (closure != null && closure.Type == null)
                    closure.BuildType();
            }

            public void EmitLocals(BoundTypeManager typeManager)
            {
                _globalLocal = IL.DeclareLocal(typeof(JsGlobal));

                EmitLoad(SpecialLocal.Runtime);
                IL.EmitCall(_runtimeGetGlobal);
                IL.Emit(OpCodes.Stloc, _globalLocal);

                _globalScopeLocal = IL.DeclareLocal(typeof(JsObject));

                EmitLoad(SpecialLocal.Runtime);
                IL.EmitCall(_runtimeGetGlobalScope);
                IL.Emit(OpCodes.Stloc, _globalScopeLocal);

                if (Closure != null)
                    ClosureLocal = IL.DeclareLocal(Closure.Type);

                foreach (var type in typeManager.Types)
                {
                    if (
                        type.Type != BoundValueType.Unset &&
                        (type.Kind == BoundTypeKind.Local || type.Kind == BoundTypeKind.Temporary)
                    )
                    {
                        var local = IL.DeclareLocal(type.Type.GetNativeType());

                        if (type.Kind == BoundTypeKind.Local)
                            local.SetLocalSymInfo(type.Name);

                        _locals.Add(type, local);

                        if (!type.DefinitelyAssigned)
                        {
                            EmitLoad(SpecialLocal.Undefined);
                            IL.Emit(OpCodes.Stloc, local);
                        }
                    }
                }
            }

            public BoundValueType EmitLoad(SpecialLocal type)
            {
                switch (type)
                {
                    case SpecialLocal.Runtime:
                        IL.Emit(OpCodes.Ldarg_0);
                        return BoundValueType.Unset;

                    case SpecialLocal.Global:
                        IL.Emit(OpCodes.Ldloc, _globalLocal);
                        return BoundValueType.Unset;

                    case SpecialLocal.GlobalScope:
                        IL.Emit(OpCodes.Ldloc, _globalScopeLocal);
                        return BoundValueType.Object;

                    case SpecialLocal.This:
                        if (_isFunction)
                        {
                            IL.Emit(OpCodes.Ldarg_1);
                            return BoundValueType.Unknown;
                        }

                        IL.Emit(OpCodes.Ldloc, _globalScopeLocal);
                        return BoundValueType.Object;

                    case SpecialLocal.Null:
                        IL.Emit(OpCodes.Ldsfld, _nullInstance);
                        return BoundValueType.Unknown;

                    case SpecialLocal.Undefined:
                        IL.Emit(OpCodes.Ldsfld, _undefinedInstance);
                        return BoundValueType.Unknown;

                    case SpecialLocal.Callee:
                        if (!_isFunction)
                            throw new InvalidOperationException();

                        IL.Emit(OpCodes.Ldarg_2);
                        return BoundValueType.Object;

                    case SpecialLocal.Closure:
                        if (!_isFunction)
                            throw new InvalidOperationException();

                        IL.Emit(OpCodes.Ldarg_3);
                        return BoundValueType.Unknown;

                    case SpecialLocal.Arguments:
                        if (!_isFunction)
                            throw new InvalidOperationException();

                        IL.Emit(OpCodes.Ldarg_S, (byte)4);
                        return BoundValueType.Unset;

                    default:
                        throw new ArgumentOutOfRangeException("type");
                }
            }

            public LocalBuilder GetLocal(IBoundType type)
            {
                return _locals[type];
            }
        }
    }
}
