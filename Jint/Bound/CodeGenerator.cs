using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading;
using Jint.Compiler;
using Jint.Native;
using Jint.Support;

namespace Jint.Bound
{
    internal partial class CodeGenerator
    {
        private const string MainMethodName = "Main";

        private static int _lastTypeId;

        private int _lastMethodId;
        private readonly HashSet<string> _compiledMethodNames = new HashSet<string>();
        private readonly JintEngine _engine;
        private Scope _scope;
        private readonly TypeBuilder _typeBuilder;
        private readonly Dictionary<BoundNode, string> _labels = new Dictionary<BoundNode, string>();

        private ILBuilder IL
        {
            get { return _scope.IL; }
        }

        public CodeGenerator(JintEngine engine)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");

            _engine = engine;

            int typeId = Interlocked.Increment(ref _lastTypeId);

            _typeBuilder = DynamicAssemblyManager.ModuleBuilder.DefineType(
                "CompiledExpression" + typeId.ToString(CultureInfo.InvariantCulture),
                 TypeAttributes.Public
            );
        }

        public Func<JintRuntime, object> BuildMainMethod(BoundProgram program)
        {
            if (program == null)
                throw new ArgumentNullException("program");

            var methodBuilder = BuildMethod(
                MainMethodName,
                typeof(object),
                new[] { typeof(JintRuntime) }
            );

            Debug.Assert(program.Body.Closure == null);

            _scope = new Scope(
                new ILBuilder(methodBuilder.GetILGenerator(), DynamicAssemblyManager.PdbGenerator),
                false,
                null,
                null,
                null
            );

            _scope.EmitLocals(program.Body.TypeManager);

            EmitStatements(program.Body.Body);

            // Ensure that we return something.

            EmitReturn(new BoundReturn(null));

            var methodInfo = _typeBuilder.CreateType().GetMethod(MainMethodName);

            var result = (Func<JintRuntime, object>)Delegate.CreateDelegate(
                typeof(Func<JintRuntime, object>),
                methodInfo
            );

            // We've build a complete script. Dump the assembly (with the right
            // constants defined) so the generated assembly can be inspected.

            DynamicAssemblyManager.FlushAssembly();

            return result;
        }

        public MethodInfo BuildFunction(BoundFunction function)
        {
            var method = DeclareFunction(function);

            var methodInfo = _typeBuilder.CreateType().GetMethod(method.Name);

            // We've build a complete script. Dump the assembly (with the right
            // constants defined) so the generated assembly can be inspected.

            DynamicAssemblyManager.FlushAssembly();

            return methodInfo;
        }

        private MethodBuilder BuildMethod(string name, Type returnType, Type[] parameterTypes)
        {
            return _typeBuilder.DefineMethod(
                name,
                MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig,
                returnType,
                parameterTypes
            );
        }

        private void EmitBox(BoundValueType type)
        {
            EmitCast(type, BoundValueType.Unknown);
        }

        private void EmitStatements(BoundBlock node)
        {
            foreach (var statement in node.Nodes)
            {
                EmitStatement(statement);
            }
        }

        private void EmitStatement(BoundStatement node)
        {
            switch (node.Kind)
            {
                case BoundKind.Block: EmitStatements((BoundBlock)node); return;
                case BoundKind.Break: EmitBreak((BoundBreak)node); return;
                case BoundKind.Continue: EmitContinue((BoundContinue)node); return;
                case BoundKind.DoWhile: throw new NotImplementedException();
                case BoundKind.Empty: throw new NotImplementedException();
                case BoundKind.ExpressionStatement: throw new NotImplementedException();
                case BoundKind.For: EmitFor((BoundFor)node); return;
                case BoundKind.ForEachIn: throw new NotImplementedException();
                case BoundKind.If: EmitIf((BoundIf)node); return;
                case BoundKind.Label: EmitLabel((BoundLabel)node); return;
                case BoundKind.Return: EmitReturn((BoundReturn)node); return;
                case BoundKind.SetAccessor: throw new NotImplementedException();
                case BoundKind.SetMember: EmitSetMember((BoundSetMember)node); return;
                case BoundKind.SetVariable: EmitSetVariable((BoundSetVariable)node); return;
                case BoundKind.Switch: throw new NotImplementedException();
                case BoundKind.Throw: throw new NotImplementedException();
                case BoundKind.Try: throw new NotImplementedException();
                case BoundKind.While: EmitWhile((BoundWhile)node); return;
                default: throw new InvalidOperationException();
            }
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

            var breakTarget = IL.DefineLabel(GetLabel(node) ?? "<>break");
            _scope.BreakTargets.Push(breakTarget);
            var continueTarget = IL.DefineLabel(GetLabel(node) ?? "<>continue");
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

            var breakTarget = IL.DefineLabel(GetLabel(node) ?? "<>break");
            _scope.BreakTargets.Push(breakTarget);
            var continueTarget = IL.DefineLabel(GetLabel(node) ?? "<>continue");
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
            // Push the value onto the stack.
            EmitCast(EmitExpression(value), variable.ValueType);

            switch (variable.Kind)
            {
                case BoundVariableKind.Local:
                case BoundVariableKind.Temporary:
                    IL.Emit(OpCodes.Stloc, _scope.GetLocal(((BoundVariable)variable).Type));
                    break;

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
                case BoundKind.DeleteMember: throw new NotImplementedException();
                case BoundKind.ExpressionBlock: return EmitExpressionBlock((BoundExpressionBlock)node);
                case BoundKind.GetMember: return EmitGetMember((BoundGetMember)node);
                case BoundKind.GetVariable: return EmitGetVariable((BoundGetVariable)node);
                case BoundKind.HasMember: throw new NotImplementedException();
                case BoundKind.New: return EmitNew((BoundNew)node);
                case BoundKind.NewBuiltIn: throw new NotImplementedException();
                case BoundKind.RegEx: throw new NotImplementedException();
                case BoundKind.Unary: return EmitUnary((BoundUnary)node);
                default: throw new InvalidOperationException();
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

            if (arguments.Count > 0)
            {
                EmitObjectArrayInit(arguments.Select(p => p.Expression));

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

            // Emit the generics array.
            if (generics.Count > 0)
                EmitObjectArrayInit(generics);
            else
                IL.Emit(OpCodes.Ldnull);

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

        private void EmitObjectArrayInit(IEnumerable<BoundExpression> expressions)
        {
            var items = expressions.ToList();

            // Create the array to hold the expressions.
            IL.EmitConstant(items.Count);
            IL.Emit(OpCodes.Newarr, typeof(object));

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                // Dup the array reference for the Stelem. We're going to leave
                // the array reference on the stack.
                IL.Emit(OpCodes.Dup);
                // Emit the index the item is at.
                IL.EmitConstant(i);
                // Emit the expression and ensure its a reference.
                EmitBox(EmitExpression(item));
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
            var function = DeclareFunction(node.Function);

            var array = IL.EmitArray(node.Function.Parameters);

            _scope.EmitLoad(SpecialLocal.Runtime);

            IL.EmitConstant(function.Name);

            // Load the method and convert to a MethodInfo.
            IL.Emit(OpCodes.Ldtoken, function);
            IL.EmitCall(_methodBaseGetMethodFromHandle);
            IL.Emit(OpCodes.Castclass, typeof(MethodInfo));

            if (_scope.ClosureLocal != null)
                IL.Emit(OpCodes.Ldloc, _scope.ClosureLocal);
            else
                IL.Emit(OpCodes.Ldnull);

            IL.Emit(OpCodes.Ldloc, array);

            // TODO: Emit the source code.
            IL.EmitConstant("");

            IL.EmitCall(_runtimeCreateFunction);

            return BoundValueType.Object;
        }

        private MethodInfo DeclareFunction(BoundFunction function)
        {
            var methodBuilder = BuildMethod(
                GetFunctionName(function),
                typeof(object),
                new[] { typeof(JintRuntime), typeof(object), typeof(JsObject), typeof(object), typeof(object[]), typeof(object[]) }
            );

            _scope = new Scope(
                new ILBuilder(methodBuilder.GetILGenerator(), DynamicAssemblyManager.PdbGenerator),
                true,
                FindScopedClosure(function.Body, _scope),
                function.Body.Locals.Single(p => p.Name == "arguments"),
                _scope
            );

            _scope.EmitLocals(function.Body.TypeManager);

            // Initialize our closure.

            var usedClosures = function.Body.TypeManager.UsedClosures;

            if (_scope.ClosureLocal != null && usedClosures != null)
            {
                EmitClosureSetup(function);

                // Emit locals for all used closures.

                EmitClosureLocals(
                    function.Body.Closure.Type,
                    _scope.ClosureLocal,
                    usedClosures
                );
            }

            // Initialize the arguments array.

            _scope.EmitLoad(SpecialLocal.Runtime);
            _scope.EmitLoad(SpecialLocal.Callee);
            _scope.EmitLoad(SpecialLocal.Arguments);
            IL.EmitCall(_runtimeCreateArguments);

            var argumentsLocal = function.Body.Locals.Single(p => p.Name == "arguments");
            if (argumentsLocal.Kind == BoundVariableKind.Local)
                IL.Emit(OpCodes.Stloc, _scope.GetLocal(argumentsLocal.Type));
            else
                throw new NotImplementedException(); // TODO: Assign to local; maybe re-use some code.

            // Emit the body.

            EmitStatements(function.Body.Body);

            // Ensure that we return something.

            EmitReturn(new BoundReturn(null));

            _scope = _scope.Parent;

            return methodBuilder;
        }

        private void EmitClosureSetup(BoundFunction function)
        {
            // Add the local to our own closure to the set.
            _scope.ClosureLocals.Add(_scope.Closure.Type, _scope.ClosureLocal);

            // Only instantiate the closure when its our closure (and not
            // just a copy of the parent closure).

            var closureType = _scope.Closure.Type;

            if (function.Body.Closure != null)
            {
                IL.Emit(OpCodes.Newobj, closureType.GetConstructors()[0]);
                IL.Emit(OpCodes.Stloc, _scope.ClosureLocal);

                // If the closure contains a link to a parent closure,
                // assign it here.

                var parentField = closureType.GetField(Expressions.Closure.ParentFieldName);
                if (parentField != null)
                {
                    _scope.EmitLoad(SpecialLocal.Closure);
                    IL.Emit(OpCodes.Castclass, closureType);
                    IL.Emit(OpCodes.Stfld, parentField);
                }

                // Initialize all fields of the closure.

                foreach (var field in closureType.GetFields())
                {
                    if (field.FieldType == typeof(object))
                    {
                        _scope.EmitLoad(SpecialLocal.Undefined);
                        IL.Emit(OpCodes.Stfld, field);
                    }
                }
            }
            else
            {
                _scope.EmitLoad(SpecialLocal.Closure);
                IL.Emit(OpCodes.Castclass, closureType);
                IL.Emit(OpCodes.Stloc, _scope.ClosureLocal);
            }
        }

        private void EmitClosureLocals(Type closureType, LocalBuilder closureLocal, ReadOnlyArray<BoundClosure> usedClosures)
        {
            // If we don't have a parent field, there is no work.

            var parentField = closureType.GetField(Expressions.Closure.ParentFieldName);
            if (parentField == null)
                return;

            var usage = new Dictionary<Type, ClosureUsage>();

            var parentUsage = DetermineClosureUsage(usage, parentField.DeclaringType, usedClosures);

            // If the parent isn't used, there is no work.

            if (parentUsage == ClosureUsage.NotUsed)
                return;

            // Emit all locals. We put the reference to the closure local on the
            // stack so the parent can be loaded from it.

            IL.Emit(OpCodes.Ldloc, closureLocal);

            while (parentField != null)
            {
                // Put the parent field on the stack. Above a check has already
                // been done to make sure at least one of the parent closures is
                // used, so we know that this is going to be popped.

                IL.Emit(OpCodes.Ldfld, parentField);

                var parentParentField = parentField.FieldType.GetField(Expressions.Closure.ParentFieldName);

                switch (usage[parentField.FieldType])
                {
                    case ClosureUsage.Directly:
                        // Declare the local for the closure and put it in the map.

                        var thisLocal = IL.DeclareLocal(parentField.FieldType);
                        _scope.ClosureLocals.Add(parentField.FieldType, thisLocal);

                        bool isLast =
                            parentParentField == null ||
                            usage[parentParentField.FieldType] == ClosureUsage.NotUsed;

                        // If we're not the last, we need to keep the reference
                        // to the closure on the stack for the next parent.

                        if (!isLast)
                            IL.Emit(OpCodes.Dup);

                        // Store the closure in the local.

                        IL.Emit(OpCodes.Stloc, thisLocal);

                        // If we're the last, we can exit here.

                        if (isLast)
                            return;
                        break;

                    case ClosureUsage.Indirectly:
                        // Nothing to do here. The parent closure has already
                        // been pushed onto the stack at the beginning of the
                        // loop, so this becomes a no-op.
                        break;

                    default:
                        // We shouldn't get here. The NotUsed case is checked in
                        // the Directly case, and the return is there.
                        throw new InvalidOperationException();
                }

                parentField = parentParentField;
            }

            Debug.Fail("We shouldn't get here because all usages should have been marked");
        }

        private ClosureUsage DetermineClosureUsage(Dictionary<Type, ClosureUsage> usage, Type closureType, ReadOnlyArray<BoundClosure> usedClosures)
        {
            // Determine the usage of the parent.

            var parentUsage = ClosureUsage.NotUsed;

            var parentField = closureType.GetField(Expressions.Closure.ParentFieldName);
            if (parentField != null)
                parentUsage = DetermineClosureUsage(usage, parentField.DeclaringType, usedClosures);

            // Determine whether this closure is used.

            var used = ClosureUsage.NotUsed;

            foreach (var closure in usedClosures)
            {
                if (closure.Type == closureType)
                {
                    used = ClosureUsage.Directly;
                    break;
                }
            }

            // This closure is used indirectly if the parent is used.

            if (used != ClosureUsage.Directly && parentUsage != ClosureUsage.Indirectly)
                used = ClosureUsage.Indirectly;

            usage[closureType] = used;

            return used;
        }

        private BoundClosure FindScopedClosure(BoundBody body, Scope scope)
        {
            if (body.Closure != null)
                return body.Closure;

            while (scope != null)
            {
                if (scope.Closure != null)
                    return scope.Closure;

                scope = scope.Parent;
            }

            return null;
        }

        private string GetFunctionName(BoundFunction function)
        {
            string name;

            if (function.Name == null)
            {
                name = (++_lastMethodId).ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                name = function.Name;

                for (int i = 0; ; i++)
                {
                    if (i > 0)
                        name = function.Name + "_" + i.ToString(CultureInfo.InvariantCulture);

                    if (!_compiledMethodNames.Contains(name))
                        break;
                }
            }

            _compiledMethodNames.Add(name);

            return name;
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
                case BoundVariableKind.Local:
                case BoundVariableKind.Temporary:
                    IL.Emit(OpCodes.Ldloc, _scope.GetLocal(((BoundVariable)variable).Type));

                    return variable.ValueType;

                case BoundVariableKind.ClosureField:
                    var closureField = (BoundClosureField)variable;

                    // Push the closure local onto the stack and get the
                    // value of the correct field.
                    IL.Emit(OpCodes.Ldloc, _scope.ClosureLocals[closureField.Closure.Type]);
                    IL.Emit(OpCodes.Ldfld, closureField.Closure.GetFieldInfo(closureField.Name));

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
                    // Push the arguments variable onto the stack.
                    EmitGetVariable(_scope.ArgumentsVariable);

                    // Get the correct member from the arguments object.
                    IL.EmitConstant(((BoundArgument)variable).Index);
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
            Closure,
            Arguments
        }

        private enum ClosureUsage
        {
            NotUsed,
            Directly,
            Indirectly
        }
    }
}
