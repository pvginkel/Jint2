using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            private readonly bool _isStatic;
            private LocalBuilder _closureLocal;

            public ILBuilder IL { get; private set; }
            public BoundClosure Closure { get; private set; }
            public Scope Parent { get; private set; }
            public Stack<NamedLabel> BreakTargets { get; private set; }
            public Stack<NamedLabel> ContinueTargets { get; private set; }
            public BoundVariable ArgumentsVariable { get; private set; }

            public Scope(ILBuilder il, bool isFunction, bool isStatic, BoundClosure closure, BoundVariable argumentsVariable, Scope parent)
            {
                IL = il;
                _isFunction = isFunction;
                _isStatic = isStatic;
                Closure = closure;
                ArgumentsVariable = argumentsVariable;
                Parent = parent;

                BreakTargets = new Stack<NamedLabel>();
                ContinueTargets = new Stack<NamedLabel>();
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
                        EmitLoadArgument(0);
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
                            EmitLoadArgument(1);
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

                        EmitLoadArgument(2);
                        return BoundValueType.Object;

                    case SpecialLocal.Arguments:
                        if (!_isFunction)
                            throw new InvalidOperationException();

                        EmitLoadArgument(3);
                        return BoundValueType.Unset;

                    default:
                        throw new ArgumentOutOfRangeException("type");
                }
            }

            private void EmitLoadArgument(int offset)
            {
                if (!_isStatic)
                    offset++;

                switch (offset)
                {
                    case 0: IL.Emit(OpCodes.Ldarg_0); break;
                    case 1: IL.Emit(OpCodes.Ldarg_1); break;
                    case 2: IL.Emit(OpCodes.Ldarg_2); break;
                    case 3: IL.Emit(OpCodes.Ldarg_3); break;
                    default: IL.Emit(OpCodes.Ldarg_S, (byte)offset); break;
                }
            }

            public LocalBuilder GetLocal(IBoundType type)
            {
                return _locals[type];
            }

            public void EmitLoadClosure(BoundClosure closure)
            {
                // Push our scoped closure onto the stack.
                if (_closureLocal != null)
                    IL.Emit(OpCodes.Ldloc, _closureLocal);
                else
                    IL.Emit(OpCodes.Ldarg_0);

                // If the request wasn't for our scoped closure, but a parent,
                // resolve the parent.
                if (Closure != closure)
                    IL.Emit(OpCodes.Ldfld, Closure.Builder.ParentFields[closure.Builder].Field);
            }

            public void SetClosureLocal(LocalBuilder closureLocal)
            {
                Debug.Assert(_closureLocal == null);

                _closureLocal = closureLocal;
            }
        }
    }
}
