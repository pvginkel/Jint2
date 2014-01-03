using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Jint.Native;
using Jint.Support;

namespace Jint.Bound
{
    partial class CodeGenerator
    {
        private abstract class ValueEmitter
        {
            protected IBoundType Type { get; private set; }
            protected ILBuilder IL { get { return Scope.IL; } }
            protected Scope Scope { get { return Generator._scope; } }
            protected CodeGenerator Generator { get; private set; }

            protected ValueEmitter(CodeGenerator generator, IBoundType type)
            {
                Generator = generator;
                Type = type;
            }

            public abstract void EmitGetValue();
            public abstract void EmitSetValue(BoundExpression expression);

            public virtual BoundValueType EmitGetMember(BoundGetMember node)
            {
                var constant = node.Index as BoundConstant;
                if (constant != null && constant.ValueType == BoundValueType.String)
                {
                    // Load the runtime.
                    Scope.EmitLoad(SpecialLocal.Runtime);
                    // Load the target to load from.
                    Generator.EmitBox(Generator.EmitExpression(node.Expression));
                    // Load the index.
                    IL.EmitConstant(Generator._identifierManager.ResolveIdentifier(
                        (string)constant.Value
                    ));
                    // Get the member by index.
                    return IL.EmitCall(_runtimeGetMemberByIndex);
                }

                return Generator.EmitOperationCall(Operation.Member, node.Expression, node.Index);
            }

            public virtual void EmitSetMember(BoundSetMember node)
            {
                var constant = node.Index as BoundConstant;
                if (constant != null && constant.ValueType == BoundValueType.String)
                {
                    // Load the target to store in.
                    Generator.EmitBox(Generator.EmitExpression(node.Expression));
                    // Load the index.
                    IL.EmitConstant(Generator._identifierManager.ResolveIdentifier((string)constant.Value));
                    // Load the expression.
                    Generator.EmitBox(Generator.EmitExpression(node.Value));
                    // Store the expression by index.
                    IL.EmitCall(_runtimeSetMemberByIndex, BoundValueType.Unset);
                }
                else
                {
                    Generator.EmitPop(Generator.EmitOperationCall(Operation.SetMember, node.Expression, node.Index, node.Value));
                }
            }

            protected bool TryGetCacheSlot(BoundExpression expression, BoundConstant constant, out int index, out FieldInfo cacheSlot)
            {
                cacheSlot = null;
                index = 0;

                string name = constant.Value as string;

                if (name != null)
                    index = Generator._identifierManager.ResolveIdentifier(name);

                if (name == null && constant.ValueType == BoundValueType.Number)
                {
                    double number = (double)constant.Value;
                    index = (int)number;
                    if (index != number || index < 0)
                        return false;

                    name = index.ToString();
                }

                cacheSlot = Generator.ResolveCacheSlot(expression, name);

                return cacheSlot != null;
            }
        }

        private abstract class LocalEmitter : ValueEmitter
        {
            public LocalBuilder Local { get; private set; }

            protected LocalEmitter(CodeGenerator generator, IBoundType type)
                : base(generator, type)
            {
            }
            protected abstract LocalBuilder DeclareLocalCore();

            public void DeclareLocal()
            {
                Local = DeclareLocalCore();
            }
        }

        private class ClosureFieldEmitter : ValueEmitter
        {
            private readonly BoundClosureField _field;

            public ClosureFieldEmitter(CodeGenerator generator, BoundClosureField field)
                : base(generator, field.Type)
            {
                _field = field;
            }

            public override void EmitGetValue()
            {
                // Push the closure onto the stack.
                Scope.EmitLoadClosure(_field.Closure);
                // Load the closure field.
                IL.Emit(OpCodes.Ldfld, _field.Builder.Field);
            }

            public override void EmitSetValue(BoundExpression expression)
            {
                // Push the closure onto the stack.
                Scope.EmitLoadClosure(_field.Closure);
                // Push the value onto the stack.
                Generator.EmitCast(Generator.EmitExpression(expression), Type.Type);
                // Set the closure field.
                IL.Emit(OpCodes.Stfld, _field.Builder.Field);
            }
        }

        private abstract class UnspeculatedEmitter : LocalEmitter
        {
            protected UnspeculatedEmitter(CodeGenerator generator, IBoundType type)
                : base(generator, type)
            {
            }

            public override void EmitGetValue()
            {
                // Load the local.
                IL.Emit(OpCodes.Ldloc, Local);
            }

            public override void EmitSetValue(BoundExpression expression)
            {
                // Push the value onto the stack.
                Generator.EmitCast(Generator.EmitExpression(expression), Type.Type);
                // Store into the local.
                IL.Emit(OpCodes.Stloc, Local);
            }
        }

        private class ObjectEmitter : UnspeculatedEmitter
        {
            public ObjectEmitter(CodeGenerator generator, IBoundType type)
                : base(generator, type)
            {
            }

            protected override LocalBuilder DeclareLocalCore()
            {
                Debug.Assert(Type.Type.GetNativeType() == typeof(JsObject));

                return IL.DeclareLocal(typeof(JsObject));
            }

            public override BoundValueType EmitGetMember(BoundGetMember node)
            {
                var constant = node.Index as BoundConstant;
                if (constant != null)
                {
                    int index;
                    FieldInfo cacheSlot;
                    if (TryGetCacheSlot(node.Expression, constant, out index, out cacheSlot))
                    {
                        // Load the target to load from.
                        Generator.EmitBox(Generator.EmitExpression(node.Expression));
                        // Load the index.
                        IL.EmitConstant(index);
                        // Load the cache slot.
                        IL.Emit(OpCodes.Ldsflda, cacheSlot);
                        // Load the value from the object.
                        return IL.EmitCall(_objectGetPropertyCached);
                    }

                    if (constant.ValueType == BoundValueType.String)
                    {
                        // Load the target to load from.
                        Generator.EmitBox(Generator.EmitExpression(node.Expression));
                        // Load the index.
                        IL.EmitConstant(Generator._identifierManager.ResolveIdentifier(
                            (string)constant.Value
                        ));
                        // Load the value from the object.
                        return IL.EmitCall(_objectGetProperty);
                    }
                }

                return Generator.EmitOperationCall(Operation.Member, node.Expression, node.Index);
            }

            public override void EmitSetMember(BoundSetMember node)
            {
                var constant = node.Index as BoundConstant;
                if (constant != null)
                {
                    int index;
                    FieldInfo cacheSlot;
                    if (TryGetCacheSlot(node.Expression, constant, out index, out cacheSlot))
                    {
                        // Load the target to store to.
                        Generator.EmitBox(Generator.EmitExpression(node.Expression));
                        // Load the index.
                        IL.EmitConstant(index);
                        // Load the value.
                        Generator.EmitBox(Generator.EmitExpression(node.Value));
                        // Load the cache slot.
                        IL.Emit(OpCodes.Ldsflda, cacheSlot);
                        // Store into the object.
                        IL.EmitCall(_objectSetPropertyCached, BoundValueType.Unset);
                        return;
                    }

                    if (constant.ValueType == BoundValueType.String)
                    {
                        // Load the target to store to.
                        Generator.EmitBox(Generator.EmitExpression(node.Expression));
                        // Load the index.
                        IL.EmitConstant(Generator._identifierManager.ResolveIdentifier(
                            (string)constant.Value
                        ));
                        // Load the value.
                        Generator.EmitBox(Generator.EmitExpression(node.Value));
                        // Store into the object.
                        IL.EmitCall(_objectSetProperty, BoundValueType.Unset);
                        return;
                    }
                }

                Generator.EmitPop(Generator.EmitOperationCall(Operation.SetMember, node.Expression, node.Index, node.Value));
            }
        }

        private class UnknownEmitter : UnspeculatedEmitter
        {
            public UnknownEmitter(CodeGenerator generator, IBoundType type)
                : base(generator, type)
            {
            }

            protected override LocalBuilder DeclareLocalCore()
            {
                return IL.DeclareLocal(Type.Type.GetNativeType());
            }
        }

        private class SpeculatedDictionaryObjectEmitter : ObjectEmitter
        {
            private static readonly MethodInfo _getProperty = typeof(DictionaryObjectBox).GetMethod("GetProperty");
            private static readonly MethodInfo _setProperty = typeof(DictionaryObjectBox).GetMethod("SetProperty");
            private static readonly MethodInfo _getValue = typeof(DictionaryObjectBox).GetProperty("Value").GetGetMethod();
            private static readonly MethodInfo _setValue = typeof(DictionaryObjectBox).GetProperty("Value").GetSetMethod();

            public SpeculatedDictionaryObjectEmitter(CodeGenerator generator, IBoundType type)
                : base(generator, type)
            {
            }

            protected override LocalBuilder DeclareLocalCore()
            {
                return IL.DeclareLocal(typeof(DictionaryObjectBox));
            }

            public override void EmitGetValue()
            {
                // Load the reference to the box.
                IL.Emit(OpCodes.Ldloca, Local);
                // Load from the box.
                IL.EmitCall(_getValue);
            }

            public override void EmitSetValue(BoundExpression expression)
            {
                // Load the reference to the box.
                IL.Emit(OpCodes.Ldloca, Local);
                // Push the value onto the stack.
                Generator.EmitCast(Generator.EmitExpression(expression), Type.Type);
                // Call set_Value.
                IL.EmitCall(_setValue);
            }

            public override BoundValueType EmitGetMember(BoundGetMember node)
            {
                var constant = node.Index as BoundConstant;
                if (constant != null && constant.ValueType == BoundValueType.String)
                {
                    int index;
                    FieldInfo cacheSlot;
                    if (TryGetCacheSlot(node.Expression, constant, out index, out cacheSlot))
                    {
                        // Load the reference to the box.
                        IL.Emit(OpCodes.Ldloca, Local);
                        // Load the index.
                        IL.EmitConstant(index);
                        // Load the cache slot.
                        IL.Emit(OpCodes.Ldsflda, cacheSlot);
                        // Load from the box by index.
                        return IL.EmitCall(_getProperty);
                    }
                }

                return base.EmitGetMember(node);
            }

            public override void EmitSetMember(BoundSetMember node)
            {
                var constant = node.Index as BoundConstant;
                if (constant != null)
                {
                    int index;
                    FieldInfo cacheSlot;
                    if (TryGetCacheSlot(node.Expression, constant, out index, out cacheSlot))
                    {
                        // Load the reference to the box.
                        IL.Emit(OpCodes.Ldloca, Local);
                        // Load the index.
                        IL.EmitConstant(index);
                        // Load the value.
                        Generator.EmitBox(Generator.EmitExpression(node.Value));
                        // Load the cache slot.
                        IL.Emit(OpCodes.Ldsflda, cacheSlot);
                        // Store into the box by index.
                        IL.EmitCall(_setProperty);
                        return;
                    }
                }

                base.EmitSetMember(node);
            }
        }

        private class SpeculatedDictionaryUnknownEmitter : UnknownEmitter
        {
            private static readonly MethodInfo _getProperty = typeof(DictionaryUnknownBox).GetMethod("GetProperty");
            private static readonly MethodInfo _setProperty = typeof(DictionaryUnknownBox).GetMethod("SetProperty");
            private static readonly MethodInfo _getValue = typeof(DictionaryUnknownBox).GetProperty("Value").GetGetMethod();
            private static readonly MethodInfo _setValue = typeof(DictionaryUnknownBox).GetProperty("Value").GetSetMethod();

            public SpeculatedDictionaryUnknownEmitter(CodeGenerator generator, IBoundType type)
                : base(generator, type)
            {
            }

            protected override LocalBuilder DeclareLocalCore()
            {
                return IL.DeclareLocal(typeof(DictionaryUnknownBox));
            }

            public override void EmitGetValue()
            {
                // Load the reference to the box.
                IL.Emit(OpCodes.Ldloca, Local);
                // Load from the box.
                IL.EmitCall(_getValue);
            }

            public override void EmitSetValue(BoundExpression expression)
            {
                // Load the reference to the local.
                IL.Emit(OpCodes.Ldloca, Local);
                // Push the value onto the stack.
                Generator.EmitCast(Generator.EmitExpression(expression), Type.Type);
                // Call set_Value.
                IL.EmitCall(_setValue);
            }

            public override BoundValueType EmitGetMember(BoundGetMember node)
            {
                var constant = node.Index as BoundConstant;
                if (constant != null)
                {
                    int index;
                    FieldInfo cacheSlot;
                    if (TryGetCacheSlot(node.Expression, constant, out index, out cacheSlot))
                    {
                        // Load the reference to the box.
                        IL.Emit(OpCodes.Ldloca, Local);
                        // Load the runtime; GetProperty on this box needs it.
                        Scope.EmitLoad(SpecialLocal.Runtime);
                        // Load the index.
                        IL.EmitConstant(index);
                        // Load the cache slot.
                        IL.Emit(OpCodes.Ldsflda, cacheSlot);
                        // Load from the box by index.
                        return IL.EmitCall(_getProperty);
                    }
                }

                return base.EmitGetMember(node);
            }

            public override void EmitSetMember(BoundSetMember node)
            {
                var constant = node.Index as BoundConstant;
                if (constant != null)
                {
                    int index;
                    FieldInfo cacheSlot;
                    if (TryGetCacheSlot(node.Expression, constant, out index, out cacheSlot))
                    {
                        // Load the reference to the box.
                        IL.Emit(OpCodes.Ldloca, Local);
                        // Load the index.
                        IL.EmitConstant(index);
                        // Load the value.
                        Generator.EmitBox(Generator.EmitExpression(node.Value));
                        // Load the cache slot.
                        IL.Emit(OpCodes.Ldsflda, cacheSlot);
                        // Store into the box.
                        IL.EmitCall(_setProperty);
                        return;
                    }
                }

                base.EmitSetMember(node);
            }
        }

        private class SpeculatedArrayObjectEmitter : ObjectEmitter
        {
            private static readonly MethodInfo _getProperty = typeof(ArrayObjectBox).GetMethod("GetProperty");
            private static readonly MethodInfo _setProperty = typeof(ArrayObjectBox).GetMethod("SetProperty");
            private static readonly MethodInfo _getValue = typeof(ArrayObjectBox).GetProperty("Value").GetGetMethod();
            private static readonly MethodInfo _setValue = typeof(ArrayObjectBox).GetProperty("Value").GetSetMethod();

            public SpeculatedArrayObjectEmitter(CodeGenerator generator, IBoundType type)
                : base(generator, type)
            {
            }

            protected override LocalBuilder DeclareLocalCore()
            {
                return IL.DeclareLocal(typeof(ArrayObjectBox));
            }

            public override void EmitGetValue()
            {
                IL.Emit(OpCodes.Ldloca, Local);
                IL.EmitCall(_getValue);
            }

            public override void EmitSetValue(BoundExpression expression)
            {
                // Load the reference to the local.
                IL.Emit(OpCodes.Ldloca, Local);
                // Push the value onto the stack.
                Generator.EmitCast(Generator.EmitExpression(expression), Type.Type);
                // Call set_Value.
                IL.EmitCall(_setValue);
            }

            public override BoundValueType EmitGetMember(BoundGetMember node)
            {
                if (node.Index.ValueType == BoundValueType.Number)
                {
                    // Load a reference to the box.
                    IL.Emit(OpCodes.Ldloca, Local);
                    // Load the index.
                    Generator.EmitExpression(node.Index);
                    // Load from the box by index.
                    return IL.EmitCall(_getProperty);
                }

                return base.EmitGetMember(node);
            }

            public override void EmitSetMember(BoundSetMember node)
            {
                if (node.Index.ValueType == BoundValueType.Number)
                {
                    // Load a reference to the box.
                    IL.Emit(OpCodes.Ldloca, Local);
                    // Load the index.
                    Generator.EmitExpression(node.Index);
                    // Load the value.
                    Generator.EmitBox(Generator.EmitExpression(node.Value));
                    // Store into the box by index.
                    IL.EmitCall(_setProperty);
                    return;
                }

                base.EmitSetMember(node);
            }
        }

        private class SpeculatedArrayUnknownEmitter : UnknownEmitter
        {
            private static readonly MethodInfo _getProperty = typeof(ArrayUnknownBox).GetMethod("GetProperty");
            private static readonly MethodInfo _setProperty = typeof(ArrayUnknownBox).GetMethod("SetProperty");
            private static readonly MethodInfo _getValue = typeof(ArrayUnknownBox).GetProperty("Value").GetGetMethod();
            private static readonly MethodInfo _setValue = typeof(ArrayUnknownBox).GetProperty("Value").GetSetMethod();

            public SpeculatedArrayUnknownEmitter(CodeGenerator generator, IBoundType type)
                : base(generator, type)
            {
            }

            protected override LocalBuilder DeclareLocalCore()
            {
                return IL.DeclareLocal(typeof(ArrayUnknownBox));
            }

            public override void EmitGetValue()
            {
                // Load a reference to the box.
                IL.Emit(OpCodes.Ldloca, Local);
                // Load from the box.
                IL.EmitCall(_getValue);
            }

            public override void EmitSetValue(BoundExpression expression)
            {
                // Load the reference to the local.
                IL.Emit(OpCodes.Ldloca, Local);
                // Push the value onto the stack.
                Generator.EmitCast(Generator.EmitExpression(expression), Type.Type);
                // Call set_Value.
                IL.EmitCall(_setValue);
            }

            public override BoundValueType EmitGetMember(BoundGetMember node)
            {
                if (node.Index.ValueType == BoundValueType.Number)
                {
                    // Load a reference to the box.
                    IL.Emit(OpCodes.Ldloca, Local);
                    // Load the runtime; this GetProperty needs it.
                    Scope.EmitLoad(SpecialLocal.Runtime);
                    // Load the index.
                    Generator.EmitExpression(node.Index);
                    // Load from the box by index.
                    return IL.EmitCall(_getProperty);
                }

                return base.EmitGetMember(node);
            }

            public override void EmitSetMember(BoundSetMember node)
            {
                if (node.Index.ValueType == BoundValueType.Number)
                {
                    // Load a reference to the box.
                    IL.Emit(OpCodes.Ldloca, Local);
                    // Load the index.
                    Generator.EmitExpression(node.Index);
                    // Load the value.
                    Generator.EmitBox(Generator.EmitExpression(node.Value));
                    // Store into the box by index.
                    IL.EmitCall(_setProperty);
                    return;
                }

                base.EmitSetMember(node);
            }
        }
    }
}
