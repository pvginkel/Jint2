using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Jint.Compiler;
using Jint.Expressions;
using Jint.Native;
using Jint.Support;

namespace Jint.Bound
{
    internal partial class CodeGenerator
    {
        private const string MainMethodName = "(global)";

        private readonly JintEngine _engine;
        private readonly IScriptBuilder _scriptBuilder;
        private Scope _scope;
        private readonly Dictionary<BoundNode, string> _labels = new Dictionary<BoundNode, string>();

        private ILBuilder IL
        {
            get { return _scope.IL; }
        }

        public CodeGenerator(JintEngine engine, IScriptBuilder scriptBuilder)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");
            if (scriptBuilder == null)
                throw new ArgumentNullException("scriptBuilder");

            _engine = engine;
            _scriptBuilder = scriptBuilder;

            _scriptBuilder.CommitClosureFields();
        }

        public Func<JintRuntime, object> BuildMainMethod(BoundProgram program)
        {
            if (program == null)
                throw new ArgumentNullException("program");

            var method = _scriptBuilder.CreateFunction(typeof(JsMain), MainMethodName, null);

            _scope = new Scope(
                method.GetILBuilder(),
                false,
                method.Method.IsStatic,
                program.Body.Closure,
                null,
                null
            );

            _scope.EmitLocals(program.Body.TypeManager);

            if (program.Body.Closure != null)
                EmitClosureSetup(program.Body);

            EmitStatements(program.Body.Body);

            // Ensure that we return something.

            EmitReturn(new BoundReturn(null, SourceLocation.Missing));

            _scriptBuilder.Commit();

            var result = (Func<JintRuntime, object>)Delegate.CreateDelegate(
                typeof(Func<JintRuntime, object>),
                method.Method
            );

            return result;
        }

        public MethodInfo BuildFunction(BoundFunction function, string sourceCode)
        {
            var method = DeclareFunction(function, _scriptBuilder, sourceCode);

            _scriptBuilder.Commit();

            return method.Method;
        }

        private void EmitBox(BoundValueType type)
        {
            EmitCast(type, BoundValueType.Unknown);
        }

        private void EmitStatements(BoundBlock node)
        {
            IL.MarkSequencePoint(node.Location);

            foreach (var statement in node.Nodes)
            {
                EmitStatement(statement);
            }
        }

        private void EmitStatement(BoundStatement node)
        {
            IL.MarkSequencePoint(node.Location);

            switch (node.Kind)
            {
                case BoundKind.Block: EmitStatements((BoundBlock)node); return;
                case BoundKind.Break: EmitBreak((BoundBreak)node); return;
                case BoundKind.Continue: EmitContinue((BoundContinue)node); return;
                case BoundKind.DoWhile: EmitDoWhile((BoundDoWhile)node); return;
                case BoundKind.Empty: return;
                case BoundKind.ExpressionStatement: EmitExpressionStatement((BoundExpressionStatement)node); return;
                case BoundKind.For: EmitFor((BoundFor)node); return;
                case BoundKind.ForEachIn: EmitForEachIn((BoundForEachIn)node); return;
                case BoundKind.If: EmitIf((BoundIf)node); return;
                case BoundKind.Label: EmitLabel((BoundLabel)node); return;
                case BoundKind.Return: EmitReturn((BoundReturn)node); return;
                case BoundKind.SetAccessor: EmitSetAccessor((BoundSetAccessor)node); return;
                case BoundKind.SetMember: EmitSetMember((BoundSetMember)node); return;
                case BoundKind.SetVariable: EmitSetVariable((BoundSetVariable)node); return;
                case BoundKind.Switch: EmitSwitch((BoundSwitch)node); return;
                case BoundKind.Throw: EmitThrow((BoundThrow)node); return;
                case BoundKind.Try: EmitTry((BoundTry)node); return;
                case BoundKind.While: EmitWhile((BoundWhile)node); return;
                default: throw new InvalidOperationException();
            }
        }

        private void EmitForEachIn(BoundForEachIn node)
        {
            // Create break and continue labels and push them onto the stack.

            var breakTarget = IL.DefineLabel(GetLabel(node));
            _scope.BreakTargets.Push(breakTarget);
            var continueTarget = IL.DefineLabel(GetLabel(node));
            _scope.ContinueTargets.Push(continueTarget);

            // Get the keys array.

            var keys = IL.DeclareLocal(typeof(object[]));
            _scope.EmitLoad(SpecialLocal.Runtime);
            EmitBox(EmitExpression(node.Expression));
            IL.EmitCall(_runtimeGetForEachKeys);
            IL.Emit(OpCodes.Stloc, keys);

            // Declare the index local.

            var index = IL.DeclareLocal(typeof(int));
            IL.EmitConstant(0);
            IL.Emit(OpCodes.Stloc, index);

            // Mark the start of the loop.

            IL.MarkLabel(continueTarget);

            // Test whether we're at the end of the loop.

            // Load the current index.
            IL.Emit(OpCodes.Ldloc, index);
            // Load the length of the array.
            IL.Emit(OpCodes.Ldloc, keys);
            IL.Emit(OpCodes.Ldlen);
            // (index >= keys.Length) -> breakTarget
            IL.Emit(OpCodes.Bge, breakTarget.Label);

            // Assign the current key to the target.

            EmitSetVariable(
                node.Target,
                new BoundEmitExpression(
                    BoundValueType.Unknown,
                    () =>
                    {
                        // Load the element out of the index array.
                        IL.Emit(OpCodes.Ldloc, keys);
                        IL.Emit(OpCodes.Ldloc, index);
                        IL.Emit(OpCodes.Ldelem_Ref);
                    }
                )
            );

            // Increment the index.

            IL.Emit(OpCodes.Ldloc, index);
            IL.EmitConstant(1);
            IL.Emit(OpCodes.Add);
            IL.Emit(OpCodes.Stloc, index);

            // Emit the body.

            EmitStatement(node.Body);

            // Jump back to the beginning of the loop.

            IL.Emit(OpCodes.Br, continueTarget.Label);

            // Mark the end of the loop.

            IL.MarkLabel(breakTarget);

            // Pop the break and continue labels to make the previous ones available.

            _scope.BreakTargets.Pop();
            _scope.ContinueTargets.Pop();
        }

        private void EmitDoWhile(BoundDoWhile node)
        {
            // Create the break and continue targets and push them onto the stack.

            var breakTarget = IL.DefineLabel(GetLabel(node));
            _scope.BreakTargets.Push(breakTarget);
            var continueTarget = IL.DefineLabel(GetLabel(node));
            _scope.ContinueTargets.Push(continueTarget);

            // Mark the start of the loop.

            var startTarget = IL.DefineLabel();
            IL.MarkLabel(startTarget);

            // Emit the body.

            EmitStatement(node.Body);

            // Begin the test.

            IL.MarkLabel(continueTarget);

            // If the test succeeds, we go again.

            EmitTest(node.Test, startTarget);

            // Mark the end of the loop.

            IL.MarkLabel(breakTarget);

            // Pop the break and continue targets to make the previous ones
            // available.

            _scope.BreakTargets.Pop();
            _scope.ContinueTargets.Pop();
        }

        private void EmitSwitch(BoundSwitch node)
        {
            // Create the label that jumps to the end of the switch.
            var after = IL.DefineLabel(GetLabel(node));
            _scope.BreakTargets.Push(after);

            // Create the bound node to get the temporary.
            var temporaryNode = new BoundGetVariable(node.Temporary);

            var bodies = new List<Tuple<Label, BoundBlock>>();

            // Emit the jump table.

            Label? defaultTarget = null;

            foreach (var @case in node.Cases)
            {
                if (@case.Expression == null)
                {
                    IL.MarkSequencePoint(@case.Location);

                    defaultTarget = IL.DefineLabel();

                    bodies.Add(Tuple.Create(defaultTarget.Value, @case.Body));
                }
                else
                {
                    var target = IL.DefineLabel();

                    IL.MarkSequencePoint(@case.Location);

                    EmitTest(
                        new BoundBinary(
                            BoundExpressionType.Equal,
                            temporaryNode,
                            @case.Expression
                        ),
                        target
                    );

                    bodies.Add(Tuple.Create(target, @case.Body));
                }
            }

            // Emit the jump to either the default block or after the switch.

            IL.Emit(OpCodes.Br, defaultTarget.GetValueOrDefault(after.Label));

            // Emit the bodies.

            foreach (var body in bodies)
            {
                IL.MarkLabel(body.Item1);

                EmitStatement(body.Item2);
            }

            // Emit the label after the switch.

            IL.MarkLabel(after);

            // Pop the break target to make the previous one available.
            _scope.BreakTargets.Pop();
        }

        private void EmitThrow(BoundThrow node)
        {
            // Create the exception object.

            EmitBox(EmitExpression(node.Expression));

            IL.Emit(OpCodes.Newobj, _exceptionConstructor);

            // Throw the exception.

            IL.Emit(OpCodes.Throw);
        }

        private void EmitTry(BoundTry node)
        {
            IL.BeginExceptionBlock();

            EmitStatement(node.Try);

            // Emit the catch block.

            if (node.Catch != null)
            {
                IL.BeginCatchBlock(typeof(Exception));

                if (node.Catch.Target == null)
                {
                    IL.Emit(OpCodes.Pop);
                }
                else
                {
                    // Store the unwrapped exception in a local.
                    var exceptionTarget = IL.DeclareLocal(typeof(Exception));
                    IL.Emit(OpCodes.Stloc, exceptionTarget);

                    // Wrap the exception.
                    _scope.EmitLoad(SpecialLocal.Runtime);
                    IL.Emit(OpCodes.Ldloc, exceptionTarget);
                    IL.EmitCall(_runtimeWrapException);

                    // Assign the exception to a local.
                    var wrappedExceptionTarget = IL.DeclareLocal(typeof(object));
                    IL.Emit(OpCodes.Stloc, wrappedExceptionTarget);

                    // Assign the wrapped exception to the target.
                    EmitSetVariable(
                        node.Catch.Target,
                        new BoundEmitExpression(
                            BoundValueType.Unknown,
                            () => IL.Emit(OpCodes.Ldloc, wrappedExceptionTarget)
                        )
                    );
                }

                // Emit the body for the catch.
                EmitStatement(node.Catch.Body);
            }

            // Emit the finally block.

            if (node.Finally != null)
            {
                IL.BeginFinallyBlock();

                EmitStatement(node.Finally.Body);
            }

            IL.EndExceptionBlock();
        }

        private void EmitSetAccessor(BoundSetAccessor node)
        {
            // Push the target onto the stack.
            EmitExpression(node.Expression);
            // Push the identifier onto the stack.
            IL.EmitConstant(_engine.Global.ResolveIdentifier(node.Name));
            // Push the getter onto the stack.
            if (node.GetFunction != null)
                EmitExpression(node.GetFunction);
            else
                IL.Emit(OpCodes.Ldnull);
            // Push the setter onto the stack.
            if (node.SetFunction != null)
                EmitExpression(node.SetFunction);
            else
                IL.Emit(OpCodes.Ldnull);
            // Call the define accessor.
            IL.EmitCall(_objectDefineAccessor);
        }

        private void EmitExpressionStatement(BoundExpressionStatement node)
        {
            EmitPop(EmitExpression(node.Expression));
        }

        private void EmitBreak(BoundBreak node)
        {
            IL.Emit(OpCodes.Br, FindLabelTarget(_scope.BreakTargets, node.Target).Label);
        }

        private void EmitContinue(BoundContinue node)
        {
            IL.Emit(OpCodes.Br, FindLabelTarget(_scope.ContinueTargets, node.Target).Label);
        }

        private NamedLabel FindLabelTarget(Stack<NamedLabel> targets, string label)
        {
            if (targets.Count == 0)
                throw new InvalidOperationException("There is not label");

            if (label != null)
            {
                // TODO: Changing the targets to a list makes this operation
                // more efficient.
                var target = targets.LastOrDefault(p => p.Name == label);
                if (target == null)
                    throw new InvalidOperationException("Cannot find break target " + label);

                return target;
            }

            return targets.Peek();
        }

        private void EmitIf(BoundIf node)
        {
            var afterTarget = IL.DefineLabel();

            if (node.Else != null)
            {
                var elseTarget = IL.DefineLabel();

                // Jump over the Then if the test fails.

                EmitTest(node.Test, elseTarget, true);

                // Emit the Then and jump over the Else.

                EmitStatement(node.Then);

                IL.Emit(OpCodes.Br, afterTarget);

                // Emit the Else.

                IL.MarkLabel(elseTarget);

                EmitStatement(node.Else);
            }
            else
            {
                // Jump over the Then if the test fails.

                EmitTest(node.Test, afterTarget, true);

                // Emit the Then.

                EmitStatement(node.Then);
            }

            // After the whole If.

            IL.MarkLabel(afterTarget);
        }

        private void EmitWhile(BoundWhile node)
        {
            // Create the break and continue targets and push them onto the stack.

            var breakTarget = IL.DefineLabel(GetLabel(node));
            _scope.BreakTargets.Push(breakTarget);
            var continueTarget = IL.DefineLabel(GetLabel(node));
            _scope.ContinueTargets.Push(continueTarget);

            // At the beginning of every iteration, perform the test.

            IL.MarkLabel(continueTarget);

            EmitTest(node.Test, breakTarget.Label, true);

            // Emit the body.

            EmitStatement(node.Body);

            // Go back to the start to perform the test again.

            IL.Emit(OpCodes.Br, continueTarget.Label);

            // After the loop.

            IL.MarkLabel(breakTarget);

            // Pop the break and continue targets to make the previous ones
            // available.

            _scope.BreakTargets.Pop();
            _scope.ContinueTargets.Pop();
        }

        private void EmitLabel(BoundLabel node)
        {
            _labels.Add(node.Statement, node.Label);
            EmitStatement(node.Statement);
        }

        private void EmitFor(BoundFor node)
        {
            // Push the break and continue targets onto the stack.

            var breakTarget = IL.DefineLabel(GetLabel(node));
            _scope.BreakTargets.Push(breakTarget);
            var continueTarget = IL.DefineLabel(GetLabel(node));
            _scope.ContinueTargets.Push(continueTarget);

            // At the start of our block, we perform any initialization.

            if (node.Initialization != null)
                EmitStatement(node.Initialization);

            // If we have a test, we perform the test at the start of every
            // iteration.

            var startTarget = IL.DefineLabel();
            IL.MarkLabel(startTarget);

            if (node.Test != null)
                EmitTest(node.Test, breakTarget.Label, true);

            // Add the body.

            EmitStatement(node.Body);

            // Increment is done at the end.

            IL.MarkLabel(continueTarget);

            if (node.Increment != null)
                EmitStatement(node.Increment);

            // Jump back to the start of the loop.

            IL.Emit(OpCodes.Br, startTarget);

            // Mark the end of the loop.

            IL.MarkLabel(breakTarget);

            // Remove the break and continue targets to make the previous ones
            // visible.

            _scope.BreakTargets.Pop();
            _scope.ContinueTargets.Pop();
        }

        private void EmitTest(BoundExpression test, Label target)
        {
            EmitTest(test, target, false);
        }

        private void EmitTest(BoundExpression test, Label target, bool inverse)
        {
            // TODO: This should be optimized. E.g. the compare expressions
            // could be changed to e.g. a Bgt.

            var type = EmitExpression(test);

            EmitCast(type, BoundValueType.Boolean);

            IL.Emit(inverse ? OpCodes.Brfalse : OpCodes.Brtrue, target);
        }

        private void EmitCast(BoundValueType source, BoundValueType target)
        {
            if (source == target)
                return;

            if (target == BoundValueType.Unknown)
            {
                if (source.IsValueType())
                    IL.Emit(OpCodes.Box, source.GetNativeType());
                return;
            }

            MethodInfo method;

            switch (target)
            {
                case BoundValueType.Boolean:
                    switch (source)
                    {
                        case BoundValueType.Boolean: throw new InvalidOperationException();
                        case BoundValueType.Number: method = _numberToBoolean; break;
                        case BoundValueType.String: method = _stringToBoolean; break;
                        default: method = _toBoolean; break;
                    }
                    break;

                case BoundValueType.Number:
                    switch (source)
                    {
                        case BoundValueType.Boolean: method = _booleanToNumber; break;
                        case BoundValueType.Number: throw new InvalidOperationException();
                        case BoundValueType.String: method = _stringToNumber; break;
                        default: method = _toNumber; break;
                    }
                    break;

                case BoundValueType.String:
                    switch (source)
                    {
                        case BoundValueType.Boolean: method = _booleanToString; break;
                        case BoundValueType.Number: method = _numberToString; break;
                        case BoundValueType.String: throw new InvalidOperationException();
                        default: method = _toString; break;
                    }
                    break;

                default: throw new ArgumentOutOfRangeException("source", String.Format("Cannot cast '{0}' to '{1}'", source, target));
            }

            IL.EmitCall(method);
        }

        private string GetLabel(BoundNode node)
        {
            string label;
            _labels.TryGetValue(node, out label);
            return label;
        }

        private void EmitSetVariable(BoundSetVariable node)
        {
            EmitSetVariable(node.Variable, node.Value);
        }

        private void EmitSetVariable(IBoundWritable variable, BoundExpression value)
        {
            switch (variable.Kind)
            {
                case BoundVariableKind.Global:
                    // Push the global scope onto the stack.
                    _scope.EmitLoad(SpecialLocal.GlobalScope);
                    // Push the index of the local onto the stack.
                    IL.EmitConstant(_engine.Global.ResolveIdentifier(((BoundGlobal)variable).Name));
                    // Push the boxed expression onto the stack.
                    EmitBox(EmitExpression(value));
                    // Set the property on the global scope.
                    IL.EmitCall(_objectSetProperty, BoundValueType.Unset);
                    break;

                case BoundVariableKind.Local:
                case BoundVariableKind.Temporary:
                    // Push the value onto the stack.
                    EmitCast(EmitExpression(value), variable.ValueType);
                    // Set the local.
                    IL.Emit(OpCodes.Stloc, _scope.GetLocal(((BoundVariable)variable).Type));
                    break;

                case BoundVariableKind.Argument:
                    var argument = (BoundArgument)variable;

                    // Push the arguments variable onto the stack.
                    if (argument.ArgumentsClosureField != null)
                        EmitGetVariable(argument.ArgumentsClosureField);
                    else
                        EmitGetVariable(_scope.ArgumentsVariable);

                    // Emit the member on the arguments object to set.
                    IL.EmitConstant(argument.Index);
                    // Push the value onto the stack.
                    EmitCast(EmitExpression(value), variable.ValueType);
                    // Set the property.
                    IL.EmitCall(_objectSetProperty);
                    return;

                case BoundVariableKind.ClosureField:
                    var closureField = (BoundClosureField)variable;
                    var closure = closureField.Closure;

                    // Push the closure onto the stack.
                    _scope.EmitLoadClosure(closure);
                    // Push the value onto the stack.
                    EmitCast(EmitExpression(value), variable.ValueType);
                    // Set the closure field.
                    IL.Emit(OpCodes.Stfld, closureField.Builder.Field);
                    return;

                default:
                    throw new NotImplementedException();
            }
        }

        private void EmitSetMember(BoundSetMember node)
        {
            // When the index is a constant string, we shortcut to
            // resolving the identifier here and inserting the ID of the
            // identifier instead of emitting the string.

            var constant = node.Index as BoundConstant;
            if (constant != null && constant.ValueType == BoundValueType.String)
                EmitSetMember(node.Expression, _engine.Global.ResolveIdentifier((string)constant.Value), node.Value);
            else
                EmitSetMember(node.Expression, node.Index, node.Value);
        }

        private void EmitSetMember(BoundExpression expression, int index, BoundExpression value)
        {
            EmitBox(EmitExpression(expression));
            IL.EmitConstant(index);
            EmitBox(EmitExpression(value));

            if (expression.ValueType == BoundValueType.Object)
                IL.EmitCall(_objectSetProperty, BoundValueType.Unset);
            else
                IL.EmitCall(_runtimeSetMemberByIndex, BoundValueType.Unset);
        }

        private void EmitSetMember(BoundExpression expression, BoundExpression index, BoundExpression value)
        {
            EmitPop(EmitOperationCall(Operation.SetMember, expression, index, value));
        }

        private void EmitPop(BoundValueType type)
        {
            if (type != BoundValueType.Unset)
                IL.Emit(OpCodes.Pop);
        }

        private void EmitReturn(BoundReturn node)
        {
            var expression = node.Expression ?? new BoundGetVariable(BoundMagicVariable.Undefined);

            EmitBox(EmitExpression(expression));
            IL.Emit(OpCodes.Ret);
        }

        private void EmitExpressions(IEnumerable<BoundExpression> nodes)
        {
            foreach (var node in nodes)
            {
                EmitExpression(node);
            }
        }

        private BoundValueType EmitExpression(BoundExpression node)
        {
            switch (node.Kind)
            {
                case BoundKind.Binary: return EmitBinary((BoundBinary)node);
                case BoundKind.Call: return EmitCall((BoundCall)node);
                case BoundKind.Constant: return EmitConstant((BoundConstant)node);
                case BoundKind.CreateFunction: return EmitCreateFunction((BoundCreateFunction)node);
                case BoundKind.DeleteMember: return EmitDeleteMember((BoundDeleteMember)node);
                case BoundKind.ExpressionBlock: return EmitExpressionBlock((BoundExpressionBlock)node);
                case BoundKind.GetMember: return EmitGetMember((BoundGetMember)node);
                case BoundKind.GetVariable: return EmitGetVariable((BoundGetVariable)node);
                case BoundKind.HasMember: return EmitHasMember((BoundHasMember)node);
                case BoundKind.New: return EmitNew((BoundNew)node);
                case BoundKind.NewBuiltIn: return EmitNewBuiltIn((BoundNewBuiltIn)node);
                case BoundKind.RegEx: return EmitRegEx((BoundRegEx)node);
                case BoundKind.Unary: return EmitUnary((BoundUnary)node);
                case BoundKind.Emit: return EmitEmit((BoundEmitExpression)node);
                default: throw new InvalidOperationException();
            }
        }

        private BoundValueType EmitHasMember(BoundHasMember node)
        {
            if (node.Expression.ValueType != BoundValueType.Object)
                _scope.EmitLoad(SpecialLocal.Runtime);

            EmitBox(EmitExpression(node.Expression));
            IL.EmitConstant(_engine.Global.ResolveIdentifier(node.Index));

            if (node.Expression.ValueType == BoundValueType.Object)
                return IL.EmitCall(_objectHasProperty);

            return IL.EmitCall(_runtimeHasMemberByIndex);
        }

        private BoundValueType EmitRegEx(BoundRegEx node)
        {
            // Push the global onto the stack.
            _scope.EmitLoad(SpecialLocal.Global);
            // Push the regular expression and options onto the stack.
            IL.EmitConstant(node.Regex);
            IL.EmitConstant(node.Options);
            // Create the regular expression object.
            IL.EmitCall(_globalCreateRegExp);

            return BoundValueType.Object;
        }

        private BoundValueType EmitDeleteMember(BoundDeleteMember node)
        {
            return EmitOperationCall(
                Operation.Delete,
                node.Expression,
                node.Index
            );
        }

        private BoundValueType EmitEmit(BoundEmitExpression node)
        {
            node.Emit();
            return node.ValueType;
        }

        private BoundValueType EmitNewBuiltIn(BoundNewBuiltIn node)
        {
            switch (node.NewBuiltInType)
            {
                case BoundNewBuiltInType.Array:
                    _scope.EmitLoad(SpecialLocal.Global);
                    IL.EmitCall(_globalCreateArray);
                    return BoundValueType.Object;

                case BoundNewBuiltInType.Object:
                    _scope.EmitLoad(SpecialLocal.Global);
                    IL.EmitCall(_globalCreateObject);
                    return BoundValueType.Object;

                default:
                    throw new InvalidOperationException();
            }
        }

        private BoundValueType EmitNew(BoundNew node)
        {
            _scope.EmitLoad(SpecialLocal.Runtime);
            EmitBox(EmitExpression(node.Expression));

            return EmitMethodArgumentsAndGenerics(
                _runtimeNew,
                node.Arguments,
                node.Generics
            );
        }

        private BoundValueType EmitCall(BoundCall node)
        {
            // Emit the arguments to the call.

            EmitExpression(node.Method);
            IL.Emit(OpCodes.Castclass, typeof(JsObject));
            _scope.EmitLoad(SpecialLocal.Runtime);
            EmitBox(EmitExpression(node.Target));

            return EmitMethodArgumentsAndGenerics(
                _objectExecute,
                node.Arguments,
                node.Generics
            );
        }

        private BoundValueType EmitMethodArgumentsAndGenerics(MethodInfo method, ReadOnlyArray<BoundCallArgument> arguments, ReadOnlyArray<BoundExpression> generics)
        {
            bool needWriteBack = arguments.Any(p => p.IsRef && p.Expression.IsAssignable());

            // Load the arguments array.

            LocalBuilder arrayLocal = null;

            if (arguments.Count > 0 || generics.Count > 0)
            {
                int count = arguments.Count;
                if (generics.Count > 0)
                    count++;

                // Create the array to hold the arguments.
                IL.EmitConstant(count);
                IL.Emit(OpCodes.Newarr, typeof(object));

                // Emit store for the elements.
                EmitArrayElementsStore(arguments.Select(p => p.Expression));

                // We smuggle the generic arguments into the last entry of the
                // arguments array. If we have generic arguments, create a
                // JsGenericArguments object to hold them and put it at the end
                // of the array.

                if (generics.Count > 0)
                {
                    // Dup the array reference for the Stelem. We're going to leave
                    // the array reference on the stack.
                    IL.Emit(OpCodes.Dup);
                    // Emit the index the item is at.
                    IL.EmitConstant(count - 1);

                    // Emit the array to hold the generic arguments.
                    IL.EmitConstant(generics.Count);
                    IL.Emit(OpCodes.Newarr, typeof(object));
                    // Store the generic arguments.
                    EmitArrayElementsStore(generics);
                    // Emit the JsGenericArguments object.
                    IL.Emit(OpCodes.Newobj, _genericArgumentsConstructor);

                    // Store the generic arguments in the array.
                    IL.Emit(OpCodes.Stelem_Ref);
                }

                // If we're doing a write back, we need to hold on to a reference
                // to the array, so dup the stack element here and store it in
                // a local.

                if (needWriteBack)
                {
                    arrayLocal = IL.DeclareLocal(typeof(object[]));
                    IL.Emit(OpCodes.Dup);
                    IL.Emit(OpCodes.Stloc, arrayLocal);
                }
            }
            else
            {
                IL.Emit(OpCodes.Ldsfld, _emptyObjectArray);
            }

            // And execute the method.

            IL.EmitCall(method);

            // The result is now on the stack, which we leave there as the result
            // of this emit.

            if (needWriteBack)
            {
                // We need to read the arguments back for when the ExecuteFunction
                // has out parameters for native calls.

                for (int i = 0; i < arguments.Count; i++)
                {
                    var argument = arguments[i];

                    if (!argument.IsRef || !argument.IsAssignable())
                        continue;

                    var valueExpression = new BoundEmitExpression(
                        BoundValueType.Unknown,
                        () =>
                        {
                            // Load the argument from the array.
                            IL.Emit(OpCodes.Ldloc, arrayLocal);
                            IL.EmitConstant(i);
                            IL.Emit(OpCodes.Ldelem_Ref);
                        }
                    );

                    if (argument.Expression.Kind == BoundKind.GetMember)
                    {
                        var getMember = (BoundGetMember)argument.Expression;

                        EmitSetMember(getMember.Expression, getMember.Index, valueExpression);
                    }
                    else
                    {
                        var getVariable = (BoundGetVariable)argument.Expression;

                        EmitSetVariable((IBoundWritable)getVariable.Variable, valueExpression);
                    }
                }
            }

            return BoundValueType.Object;
        }

        private void EmitArrayElementsStore(IEnumerable<BoundExpression> elements)
        {
            var items = elements.ToList();

            for (int i = 0; i < items.Count; i++)
            {
                // Dup the array reference for the Stelem. We're going to leave
                // the array reference on the stack.
                IL.Emit(OpCodes.Dup);
                // Emit the index the item is at.
                IL.EmitConstant(i);
                // Emit the expression and ensure its a reference.
                EmitBox(EmitExpression(items[i]));
                // Store the element.
                IL.Emit(OpCodes.Stelem_Ref);
            }
        }

        private BoundValueType EmitExpressionBlock(BoundExpressionBlock node)
        {
            EmitStatement(node.Body);

            EmitGetVariable(node.Result);

            return node.ValueType;
        }

        private BoundValueType EmitUnary(BoundUnary node)
        {
            if (node.Operation == BoundExpressionType.Void)
            {
                EmitPop(EmitExpression(node.Operand));

                return _scope.EmitLoad(SpecialLocal.Undefined);
            }

            return EmitOperationCall(GetOperation(node.Operation), node.Operand);
        }

        private BoundValueType EmitBinary(BoundBinary node)
        {
            return EmitOperationCall(GetOperation(node.Operation), node.Left, node.Right);
        }

        private Operation GetOperation(BoundExpressionType type)
        {
            switch (type)
            {
                case BoundExpressionType.Add: return Operation.Add;
                case BoundExpressionType.BitwiseAnd: return Operation.BitwiseAnd;
                case BoundExpressionType.BitwiseExclusiveOr: return Operation.BitwiseExclusiveOr;
                case BoundExpressionType.BitwiseNot: return Operation.BitwiseNot;
                case BoundExpressionType.BitwiseOr: return Operation.BitwiseOr;
                case BoundExpressionType.Divide: return Operation.Divide;
                case BoundExpressionType.Equal: return Operation.Equal;
                case BoundExpressionType.GreaterThan: return Operation.GreaterThan;
                case BoundExpressionType.GreaterThanOrEqual: return Operation.GreaterThanOrEqual;
                case BoundExpressionType.In: return Operation.In;
                case BoundExpressionType.InstanceOf: return Operation.InstanceOf;
                case BoundExpressionType.LeftShift: return Operation.LeftShift;
                case BoundExpressionType.LessThan: return Operation.LessThan;
                case BoundExpressionType.LessThanOrEqual: return Operation.LessThanOrEqual;
                case BoundExpressionType.Modulo: return Operation.Modulo;
                case BoundExpressionType.Multiply: return Operation.Multiply;
                case BoundExpressionType.Negate: return Operation.Negate;
                case BoundExpressionType.Not: return Operation.Not;
                case BoundExpressionType.NotEqual: return Operation.NotEqual;
                case BoundExpressionType.NotSame: return Operation.NotSame;
                case BoundExpressionType.RightShift: return Operation.RightShift;
                case BoundExpressionType.Same: return Operation.Same;
                case BoundExpressionType.Subtract: return Operation.Subtract;
                case BoundExpressionType.TypeOf: return Operation.TypeOf;
                case BoundExpressionType.UnaryPlus: return Operation.UnaryPlus;
                case BoundExpressionType.UnsignedRightShift: return Operation.UnsignedRightShift;
                default: throw new ArgumentOutOfRangeException("type");
            }
        }

        private BoundValueType EmitCreateFunction(BoundCreateFunction node)
        {
            ITypeBuilder typeBuilder = _scriptBuilder;
            if (_scope.Closure != null)
                typeBuilder = _scope.Closure.Builder;

            var location = node.Function.Location;
            string sourceCode = null;
            if (location != null)
                sourceCode = location.GetSourceCode();

            var function = DeclareFunction(node.Function, typeBuilder, sourceCode);

            var array = IL.EmitArray(node.Function.Parameters);

            _scope.EmitLoad(SpecialLocal.Runtime);

            IL.EmitConstant(node.Function.Name ?? String.Empty);

            // Create a delegate to the method. First load the instance reference.
            if (_scope.Closure == null)
                IL.Emit(OpCodes.Ldnull);
            else
                _scope.EmitLoadClosure(_scope.Closure);
            // Load the function.
            IL.Emit(OpCodes.Ldftn, function.Method);
            // Construct the delegate.
            IL.Emit(OpCodes.Newobj, _functionConstructor);

            IL.Emit(OpCodes.Ldloc, array);

            IL.EmitCall(_runtimeCreateFunction);

            return BoundValueType.Object;
        }

        private IFunctionBuilder DeclareFunction(BoundFunction function, ITypeBuilder typeBuilder, string sourceCode)
        {
            var method = typeBuilder.CreateFunction(typeof(JsFunction), function.Name, sourceCode);

            var argumentsLocal = (BoundVariable)function.Body.Locals.SingleOrDefault(p => p.Name == "arguments");
            if (argumentsLocal == null)
            {
                Debug.Assert(function.Body.Closure != null);
                argumentsLocal = function.Body.Closure.Fields[Expressions.Closure.ArgumentsFieldName];
            }

            _scope = new Scope(
                method.GetILBuilder(),
                true,
                method.Method.IsStatic,
                function.Body.ScopedClosure,
                argumentsLocal,
                _scope
            );

            _scope.EmitLocals(function.Body.TypeManager);

            // Instantiate the closure if we own it.
            if (function.Body.Closure != null)
                EmitClosureSetup(function.Body);

            // Put the closure local onto the stack if we're going to store
            // the arguments in the closure.
            if (argumentsLocal.Kind == BoundVariableKind.ClosureField)
                _scope.EmitLoadClosure(_scope.Closure);

            // Initialize the arguments array.

            _scope.EmitLoad(SpecialLocal.Runtime);
            _scope.EmitLoad(SpecialLocal.Callee);
            _scope.EmitLoad(SpecialLocal.Arguments);

            IL.EmitCall(_runtimeCreateArguments);

            if (argumentsLocal.Kind == BoundVariableKind.ClosureField)
                IL.Emit(OpCodes.Stfld, ((BoundClosureField)argumentsLocal).Builder.Field);
            else
                IL.Emit(OpCodes.Stloc, _scope.GetLocal(argumentsLocal.Type));

            // Emit the body.

            EmitStatements(function.Body.Body);

            // Ensure that we return something.

            EmitReturn(new BoundReturn(null, SourceLocation.Missing));

            // Put a debug location at the end of the function.

            if (function.Location != null)
            {
                IL.MarkSequencePoint(new SourceLocation(
                    function.Location.EndOffset,
                    function.Location.EndLine,
                    function.Location.EndColumn,
                    function.Location.EndOffset + 1,
                    function.Location.EndLine,
                    function.Location.EndColumn + 1,
                    null
                ));
            }

            _scope = _scope.Parent;

            return method;
        }

        private void EmitClosureSetup(BoundBody body)
        {
            Debug.Assert(body.Closure != null);

            var closure = body.Closure;

            // Check whether the constructor expects a parent.
            if (body.Closure.Parent != null)
                IL.Emit(OpCodes.Ldarg_0);

            // Instantiate the closure.
            IL.Emit(OpCodes.Newobj, closure.Builder.Constructor);

            // Store the reference to the closure in the scope.

            var closureLocal = IL.DeclareLocal(closure.Builder.Type);
            _scope.SetClosureLocal(closureLocal);
            IL.Emit(OpCodes.Stloc, closureLocal);
        }

        private BoundValueType EmitGetMember(BoundGetMember node)
        {
            // When the index is a constant string, we shortcut to
            // resolving the identifier here and inserting the ID of the
            // identifier instead of emitting the string.

            var constant = node.Index as BoundConstant;
            if (constant != null && constant.ValueType == BoundValueType.String)
                return EmitGetMember(node.Expression, _engine.Global.ResolveIdentifier((string)constant.Value));

            return EmitGetMember(node.Expression, node.Index);
        }

        private BoundValueType EmitGetMember(BoundExpression expression, int index)
        {
            if (expression.ValueType != BoundValueType.Object)
                _scope.EmitLoad(SpecialLocal.Runtime);

            EmitBox(EmitExpression(expression));
            IL.EmitConstant(index);

            if (expression.ValueType == BoundValueType.Object)
                return IL.EmitCall(_objectGetProperty);

            return IL.EmitCall(_runtimeGetMemberByIndex);
        }

        private BoundValueType EmitGetMember(BoundExpression expression, BoundExpression index)
        {
            return EmitOperationCall(Operation.Member, expression, index);
        }

        private BoundValueType EmitGetVariable(BoundGetVariable node)
        {
            return EmitGetVariable(node.Variable);
        }

        private BoundValueType EmitGetVariable(IBoundReadable variable)
        {
            switch (variable.Kind)
            {
                case BoundVariableKind.Global:
                    // Push the global scope onto the stack.
                    _scope.EmitLoad(SpecialLocal.GlobalScope);
                    // Push the index onto the stack.
                    IL.EmitConstant(_engine.Global.ResolveIdentifier(((BoundGlobal)variable).Name));
                    // Get the property from the global scope.
                    return IL.EmitCall(_objectGetProperty);

                case BoundVariableKind.Local:
                case BoundVariableKind.Temporary:
                    IL.Emit(OpCodes.Ldloc, _scope.GetLocal(((BoundVariable)variable).Type));
                    return variable.ValueType;

                case BoundVariableKind.ClosureField:
                    var closureField = (BoundClosureField)variable;
                    var closure = closureField.Closure;

                    // Push the closure onto the stack.
                    _scope.EmitLoadClosure(closure);
                    // Load the closure field.
                    IL.Emit(OpCodes.Ldfld, closure.Fields[closureField.Name].Builder.Field);

                    return variable.ValueType;

                case BoundVariableKind.Magic:
                    switch (((BoundMagicVariable)variable).VariableType)
                    {
                        case BoundMagicVariableType.Global: return _scope.EmitLoad(SpecialLocal.GlobalScope);
                        case BoundMagicVariableType.This: return _scope.EmitLoad(SpecialLocal.This);
                        case BoundMagicVariableType.Null: return _scope.EmitLoad(SpecialLocal.Null);
                        case BoundMagicVariableType.Undefined: return _scope.EmitLoad(SpecialLocal.Undefined);
                        default: throw new NotImplementedException();
                    }

                case BoundVariableKind.Argument:
                    var argument = (BoundArgument)variable;

                    // Push the arguments variable onto the stack.
                    if (argument.ArgumentsClosureField != null)
                        EmitGetVariable(argument.ArgumentsClosureField);
                    else
                        EmitGetVariable(_scope.ArgumentsVariable);

                    // Get the correct member from the arguments object.
                    IL.EmitConstant(argument.Index);
                    IL.EmitCall(_objectGetProperty);

                    return BoundValueType.Unknown;

                default:
                    throw new NotImplementedException();
            }
        }

        private BoundValueType EmitConstant(BoundConstant node)
        {
            IL.EmitConstant(node.Value);
            return node.ValueType;
        }

        private enum SpecialLocal
        {
            Runtime,
            Global,
            GlobalScope,
            This,
            Null,
            Undefined,
            Callee,
            Arguments
        }
    }
}
