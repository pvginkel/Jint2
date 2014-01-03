using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Jint.Compiler;
using Jint.Native;
using Jint.Support;

namespace Jint.Bound
{
    partial class CodeGenerator
    {
        private class Scope
        {
            private readonly Dictionary<IBoundType, ValueEmitter> _locals = new Dictionary<IBoundType, ValueEmitter>();
            private LocalBuilder _globalLocal;
            private LocalEmitter _thisLocal;
            private readonly bool _isFunction;
            private readonly BoundBody _body;
            private readonly bool _isStatic;
            private LocalBuilder _closureLocal;
            private int _tryCatchNesting;
            private readonly Dictionary<BoundArgument, BoundVariable> _arguments;
            private Dictionary<BoundMagicVariableType, IBoundType> _magicTypes;

            public ILBuilder IL { get; private set; }
            public BoundClosureField ArgumentsClosureField { get; private set; }
            public CodeGenerator Generator { get; private set; }
            public Scope Parent { get; private set; }
            public Stack<NamedLabel> BreakTargets { get; private set; }
            public Stack<NamedLabel> ContinueTargets { get; private set; }
            public ValueEmitter ArgumentsEmitter { get; private set; }
            public ITypeBuilder TypeBuilder { get; private set; }
            public ExceptionalReturn ExceptionalReturn { get; private set; }
            public LocalEmitter GlobalScopeEmitter { get; private set; }

            public BoundClosure Closure
            {
                get { return _body.ScopedClosure; }
            }

            public bool IsStrict
            {
                get { return (_body.Flags & BoundBodyFlags.Strict) != 0; }
            }

            public bool InTryCatch
            {
                get { return _tryCatchNesting > 0; }
            }

            public Scope(CodeGenerator generator, ILBuilder il, bool isFunction, BoundBody body, BoundClosureField argumentsClosureField, ITypeBuilder typeBuilder, Scope parent)
            {
                IL = il;
                ArgumentsClosureField = argumentsClosureField;
                Generator = generator;
                _isFunction = isFunction;
                _body = body;
                if (argumentsClosureField != null)
                    ArgumentsEmitter = new ClosureFieldEmitter(generator, argumentsClosureField);
                TypeBuilder = typeBuilder;
                Parent = parent;

                _isStatic = TypeBuilder is IScriptBuilder;
                if (body.MappedArguments != null)
                    _arguments = body.MappedArguments.ToDictionary(p => p.Argument, p => p.Mapped);

                BreakTargets = new Stack<NamedLabel>();
                ContinueTargets = new Stack<NamedLabel>();
            }

            public BoundVariable GetMappedArgument(BoundArgument argument)
            {
                if (_arguments != null)
                {
                    BoundVariable result;
                    _arguments.TryGetValue(argument, out result);
                    return result;
                }

                return null;
            }

            public void EmitLocals(BoundTypeManager typeManager)
            {
                _magicTypes = _body.TypeManager.MagicTypes.ToDictionary(p => p.MagicType, p => p.Type);

                // Create the arguments local if it's required and there isn't
                // a closure field for it.

                if ((_body.Flags & BoundBodyFlags.ArgumentsReferenced) != 0 && ArgumentsEmitter == null)
                {
                    var type = _magicTypes[BoundMagicVariableType.Arguments];

                    var argumentsEmitter = CreateEmitter(type);
                    argumentsEmitter.DeclareLocal();

                    ArgumentsEmitter = argumentsEmitter;

                    _locals.Add(type, argumentsEmitter);
                }

                if ((_body.Flags & BoundBodyFlags.GlobalReferenced) != 0)
                {
                    _globalLocal = IL.DeclareLocal(typeof(JsGlobal));

                    EmitLoad(SpecialLocal.Runtime);
                    IL.EmitCall(_runtimeGetGlobal);
                    IL.Emit(OpCodes.Stloc, _globalLocal);
                }

                if ((_body.Flags & BoundBodyFlags.GlobalScopeReferenced) != 0)
                {
                    var type = _magicTypes[BoundMagicVariableType.Global];

                    GlobalScopeEmitter = CreateEmitter(type);
                    GlobalScopeEmitter.DeclareLocal();
                    GlobalScopeEmitter.EmitSetValue(new BoundEmitExpression(
                        BoundValueType.Object,
                        () =>
                        {
                            EmitLoad(SpecialLocal.Runtime);
                            IL.EmitCall(_runtimeGetGlobalScope);
                        }
                    ));

                    _locals.Add(type, GlobalScopeEmitter);
                }

                if ((_body.Flags & BoundBodyFlags.ThisReferenced) != 0)
                {
                    // We can't set assign to _thisLocal because then EmitLoad
                    // would return the local.

                    var type = _magicTypes[BoundMagicVariableType.This];

                    var thisLocal = CreateEmitter(type);
                    thisLocal.DeclareLocal();
                    thisLocal.EmitSetValue(new BoundEmitExpression(
                        BoundValueType.Unknown,
                        () => EmitLoad(SpecialLocal.This)
                    ));

                    _thisLocal = thisLocal;

                    _locals.Add(type, _thisLocal);
                }

                var getUnknown = new BoundGetVariable(BoundMagicVariable.Undefined);

                foreach (var type in typeManager.Types)
                {
                    if (type.Type == BoundValueType.Unset)
                        continue;

                    if (type.Kind == BoundTypeKind.Local || type.Kind == BoundTypeKind.Temporary)
                    {
                        var emitter = CreateEmitter(type);

                        emitter.DeclareLocal();

                        if (type.Kind == BoundTypeKind.Local)
                            emitter.Local.SetLocalSymInfo(type.Name);

                        if (!type.DefinitelyAssigned)
                            emitter.EmitSetValue(getUnknown);

                        _locals.Add(type, emitter);
                    }
                    else if (type.Kind == BoundTypeKind.ClosureField)
                    {
                        _locals.Add(type, new ClosureFieldEmitter(
                            Generator,
                            Closure.Fields[type.Name]
                        ));
                    }
                }
            }

            private LocalEmitter CreateEmitter(IBoundType type)
            {
                switch (type.SpeculatedType)
                {
                    case SpeculatedType.Array:
                        Debug.Assert(type.Type == BoundValueType.Object || type.Type == BoundValueType.Unknown);

                        if (type.Type == BoundValueType.Object)
                            return new SpeculatedArrayObjectEmitter(Generator, type);
                        return new SpeculatedArrayUnknownEmitter(Generator, type);

                    case SpeculatedType.Object:
                        Debug.Assert(type.Type == BoundValueType.Object || type.Type == BoundValueType.Unknown);

                        if (type.Type == BoundValueType.Object)
                            return new SpeculatedDictionaryObjectEmitter(Generator, type);
                        return new SpeculatedDictionaryUnknownEmitter(Generator, type);

                    default:
                        if (type.Type == BoundValueType.Object)
                            return new ObjectEmitter(Generator, type);
                        return new UnknownEmitter(Generator, type);
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
                        Debug.Assert((_body.Flags & BoundBodyFlags.GlobalReferenced) != 0);

                        IL.Emit(OpCodes.Ldloc, _globalLocal);
                        return BoundValueType.Unset;

                    case SpecialLocal.GlobalScope:
                        Debug.Assert((_body.Flags & BoundBodyFlags.GlobalScopeReferenced) != 0);

                        GlobalScopeEmitter.EmitGetValue();
                        return BoundValueType.Object;

                    case SpecialLocal.This:
                        Debug.Assert((_body.Flags & BoundBodyFlags.ThisReferenced) != 0);

                        if (_isFunction)
                        {
                            if (_thisLocal != null)
                                _thisLocal.EmitGetValue();
                            else
                                EmitLoadArgument(1);
                            return BoundValueType.Unknown;
                        }

                        return EmitLoad(SpecialLocal.GlobalScope);

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

                    case SpecialLocal.ArgumentsRaw:
                        if (!_isFunction)
                            throw new InvalidOperationException();

                        EmitLoadArgument(3);
                        return BoundValueType.Unset;

                    case SpecialLocal.Arguments:
                        Debug.Assert((_body.Flags & BoundBodyFlags.ArgumentsReferenced) != 0);

                        if (!_isFunction)
                            throw new InvalidOperationException();

                        ArgumentsEmitter.EmitGetValue();
                        return BoundValueType.Object;

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

            public ValueEmitter GetEmitter(IBoundReadable readable)
            {
                IBoundType type = null;

                var variable = readable as BoundVariable;
                if (variable != null)
                {
                    type = variable.Type;
                }
                else
                {
                    var magic = readable as BoundMagicVariable;
                    if (magic != null)
                        _magicTypes.TryGetValue(magic.VariableType, out type);
                }

                if (type == null)
                    return null;

                var scope = this;

                while (scope != null)
                {
                    ValueEmitter emitter;
                    if (scope._locals.TryGetValue(type, out emitter))
                        return emitter;

                    scope = scope.Parent;
                }

                return null;
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

            public Scope FindScope(BoundClosure closure)
            {
                // Here we find the scope that the closure actually belongs to,
                // not the scope the closure is scoped in. This method is used
                // to get the scope the closure actually belongs to to be able to
                // get information that is cached in that scope, e.g. the
                // _arguments and ArgumentsClosureField.

                var scope = this;

                while (scope._body.Closure != closure)
                {
                    scope = scope.Parent;
                }

                return scope;
            }

            public void SetClosureLocal(LocalBuilder closureLocal)
            {
                Debug.Assert(_closureLocal == null);

                _closureLocal = closureLocal;
            }

            public ExceptionalReturn GetExceptionalReturn()
            {
                if (ExceptionalReturn == null)
                {
                    ExceptionalReturn = new ExceptionalReturn(
                        IL.DefineLabel(),
                        IL.DeclareLocal(typeof(object))
                    );
                }

                return ExceptionalReturn;
            }

            public void EntryTryCatch()
            {
                _tryCatchNesting++;
            }

            public void LeaveTryCatch()
            {
                _tryCatchNesting--;
            }
        }

        private class ExceptionalReturn
        {
            public Label Label { get; private set; }
            public LocalBuilder Local { get; private set; }

            public ExceptionalReturn(Label label, LocalBuilder local)
            {
                if (local == null)
                    throw new ArgumentNullException("local");

                Label = label;
                Local = local;
            }
        }
    }
}
