using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading;
using Jint.Compiler;
using Jint.Expressions;
using Jint.Native;
using Jint.Support;

namespace Jint.Bound
{
    internal partial class CodeGenerator
    {
        private const string MainMethodName = "Main";

        private static int _lastTypeId;
        private static int _lastClosureId;

        private int _lastMethodId;
        private readonly HashSet<string> _compiledMethodNames = new HashSet<string>();
        private readonly JintEngine _engine;
        private Scope _scope;
        private readonly TypeBuilder _typeBuilder;
        private readonly Dictionary<BoundNode, string> _labels = new Dictionary<BoundNode, string>();
        private readonly ISymbolDocumentWriter _document;
        private readonly Dictionary<BoundFunction, MethodInfo> _functions = new Dictionary<BoundFunction, MethodInfo>();
        private readonly Dictionary<BoundClosure, RuntimeClosure> _closures = new Dictionary<BoundClosure, RuntimeClosure>();

        private ILBuilder IL
        {
            get { return _scope.IL; }
        }

        public CodeGenerator(JintEngine engine, string fileName)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");

            _engine = engine;

            int typeId = Interlocked.Increment(ref _lastTypeId);

            _typeBuilder = DynamicAssemblyManager.ModuleBuilder.DefineType(
                "CompiledExpression" + typeId.ToString(CultureInfo.InvariantCulture),
                 TypeAttributes.Public
            );

            if (fileName != null)
            {
                _document = DynamicAssemblyManager.ModuleBuilder.DefineDocument(
                    Path.GetFullPath(fileName),
                    SymLanguageType.JScript,
                    SymLanguageVendor.Microsoft,
                    SymDocumentType.Text
                );
            }
        }

        public Func<JintRuntime, object> BuildMainMethod(BoundProgram program)
        {
            if (program == null)
                throw new ArgumentNullException("program");

            BuildFunctionsAndClosures(program.Body);

            var methodBuilder = BuildMethod(
                null,
                MainMethodName,
                typeof(object),
                new[] { typeof(JintRuntime) }
            );

            Debug.Assert(program.Body.Closure == null);

            _scope = new Scope(
                new ILBuilder(methodBuilder.GetILGenerator(), _document),
                false,
                true,
                null,
                null,
                null
            );

            _scope.EmitLocals(program.Body.TypeManager);

            EmitStatements(program.Body.Body);

            // Ensure that we return something.

            EmitReturn(new BoundReturn(null, SourceLocation.Missing));

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
            BuildFunctionsAndClosures(function.Body);

            var method = DeclareFunction(function, null);

            var methodInfo = _typeBuilder.CreateType().GetMethod(method.Name);

            // We've build a complete script. Dump the assembly (with the right
            // constants defined) so the generated assembly can be inspected.

            DynamicAssemblyManager.FlushAssembly();

            return methodInfo;
        }

        private void BuildFunctionsAndClosures(BoundBody body)
        {
            var functions = FunctionGatherer.Gather(body);

            // Group the functions by closure.

            var functionClosures = new Dictionary<BoundFunction, BoundClosure>();
            var closures = new Dictionary<BoundClosure, List<BoundFunction>>();
            var closuresOrder = new List<BoundClosure>();

            foreach (var function in functions)
            {
                BoundClosure parentClosure;
                if (function.Body.Closure != null)
                    parentClosure = function.Body.Closure.Parent;
                else
                    parentClosure = function.Body.ScopedClosure;

                if (parentClosure != null)
                {
                    functionClosures.Add(function, parentClosure);

                    List<BoundFunction> closureFunctions;
                    if (!closures.TryGetValue(parentClosure, out closureFunctions))
                    {
                        closureFunctions = new List<BoundFunction>();
                        closures.Add(parentClosure, closureFunctions);
                        closuresOrder.Add(parentClosure);
                    }

                    closureFunctions.Add(function);
                }
            }

            // Create the types for all closures.

            foreach (var closure in closuresOrder)
            {
                EmitClosure(closure, closures[closure]);
            }

            // Build all functions. This is done in reverse order, so the
            // functions can reference each other.

            foreach (var function in functions.Reverse())
            {
                TypeBuilder closureType;

                BoundClosure closure;
                if (functionClosures.TryGetValue(function, out closure))
                    closureType = _closures[closure].Type;
                else
                    closureType = null;

                _functions.Add(function, DeclareFunction(function, closureType));
            }

            // And create all types.

            foreach (var closure in _closures.Values)
            {
                closure.Type.CreateType();
            }
        }

        private void EmitClosure(BoundClosure closure, IEnumerable<BoundFunction> functions)
        {
            int id = Interlocked.Increment(ref _lastClosureId);

            var dynamicType = DynamicAssemblyManager.ModuleBuilder.DefineType(
                "<>JintClosure_" + id.ToString(CultureInfo.InvariantCulture),
                TypeAttributes.SpecialName
            );

            var fields = new Dictionary<string, FieldInfo>();

            foreach (var field in closure.Fields.OrderBy(p => p.Name))
            {
                var attributes = FieldAttributes.Public;
                Type fieldType;

                if (field.Name == Expressions.Closure.ParentFieldName)
                {
                    attributes |= FieldAttributes.InitOnly;
                    fieldType = _closures[closure.Parent].Type;
                }
                else if (field.Name == Expressions.Closure.ArgumentsFieldName)
                {
                    // TODO: This should have been set correctly, but field.Type
                    // can be Unset at this point.
                    fieldType = typeof(JsObject);
                }
                else
                {
                    fieldType = field.ValueType.GetNativeType();
                }

                fields.Add(
                    field.Name,
                    dynamicType.DefineField(field.Name, fieldType, attributes)
                );
            }

            _closures.Add(closure, new RuntimeClosure(
                closure,
                dynamicType,
                EmitClosureConstructor(closure, dynamicType, fields),
                fields
            ));
        }

        private ConstructorBuilder EmitClosureConstructor(BoundClosure closure, TypeBuilder closureType, Dictionary<string, FieldInfo> fields)
        {
            var parameterTypes = Type.EmptyTypes;

            if (closure.Parent != null)
                parameterTypes = new[] { _closures[closure.Parent].Type };

            var constructor = closureType.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                parameterTypes
            );

            var il = new ILBuilder(constructor.GetILGenerator(), null);

            // Call the base constructor.

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, _objectConstructor);

            // Initialize the parent field if we have one.

            if (closure.Parent != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, fields[Expressions.Closure.ParentFieldName]);
            }

            // Initialize object fields to undefined.

            foreach (var field in fields)
            {
                if (field.Value.FieldType == typeof(object))
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldsfld, _undefinedInstance);
                    il.Emit(OpCodes.Stfld, field.Value);
                }
            }

            // Return from the constructor.

            il.Emit(OpCodes.Ret);

            return constructor;
        }

        private MethodBuilder BuildMethod(TypeBuilder typeBuilder, string name, Type returnType, Type[] parameterTypes)
        {
            var attributes = MethodAttributes.Public | MethodAttributes.HideBySig;
            if (typeBuilder == null)
                attributes |= MethodAttributes.Static;

            return (typeBuilder ?? _typeBuilder).DefineMethod(
                name,
                attributes,
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
                case BoundKind.DoWhile: throw new NotImplementedException();
                case BoundKind.Empty: return;
                case BoundKind.ExpressionStatement: EmitExpressionStatement((BoundExpressionStatement)node); return;
                case BoundKind.For: EmitFor((BoundFor)node); return;
                case BoundKind.ForEachIn: throw new NotImplementedException();
                case BoundKind.If: EmitIf((BoundIf)node); return;
                case BoundKind.Label: EmitLabel((BoundLabel)node); return;
                case BoundKind.Return: EmitReturn((BoundReturn)node); return;
                case BoundKind.SetAccessor: EmitSetAccessor((BoundSetAccessor)node); return;
                case BoundKind.SetMember: EmitSetMember((BoundSetMember)node); return;
                case BoundKind.SetVariable: EmitSetVariable((BoundSetVariable)node); return;
                case BoundKind.Switch: throw new NotImplementedException();
                case BoundKind.Throw: throw new NotImplementedException();
                case BoundKind.Try: throw new NotImplementedException();
                case BoundKind.While: EmitWhile((BoundWhile)node); return;
                default: throw new InvalidOperationException();
            }
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
            switch (variable.Kind)
            {
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
                    _scope.EmitLoadClosure(_closures[closure].Type);
                    // Push the value onto the stack.
                    EmitCast(EmitExpression(value), variable.ValueType);
                    // Set the closure field.
                    IL.Emit(OpCodes.Stfld, _closures[closure].Fields[closureField.Name]);
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
                case BoundKind.DeleteMember: throw new NotImplementedException();
                case BoundKind.ExpressionBlock: return EmitExpressionBlock((BoundExpressionBlock)node);
                case BoundKind.GetMember: return EmitGetMember((BoundGetMember)node);
                case BoundKind.GetVariable: return EmitGetVariable((BoundGetVariable)node);
                case BoundKind.HasMember: throw new NotImplementedException();
                case BoundKind.New: return EmitNew((BoundNew)node);
                case BoundKind.NewBuiltIn: return EmitNewBuiltIn((BoundNewBuiltIn)node);
                case BoundKind.RegEx: throw new NotImplementedException();
                case BoundKind.Unary: return EmitUnary((BoundUnary)node);
                default: throw new InvalidOperationException();
            }
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
            var function = _functions[node.Function];

            var array = IL.EmitArray(node.Function.Parameters);

            _scope.EmitLoad(SpecialLocal.Runtime);

            IL.EmitConstant(function.Name);

            // Create a delegate to the method. First load the instance reference.
            if (function.IsStatic)
                IL.Emit(OpCodes.Ldnull);
            else
                _scope.EmitLoadClosure(_closures[_scope.Closure].Type);
            // Load the function.
            IL.Emit(OpCodes.Ldftn, function);
            // Construct the delegate.
            IL.Emit(OpCodes.Newobj, _functionConstructor);

            IL.Emit(OpCodes.Ldloc, array);

            // TODO: Emit the source code.
            IL.EmitConstant("");

            IL.EmitCall(_runtimeCreateFunction);

            return BoundValueType.Object;
        }

        private MethodInfo DeclareFunction(BoundFunction function, TypeBuilder typeBuilder)
        {
            var methodBuilder = BuildMethod(
                typeBuilder,
                GetFunctionName(function),
                typeof(object),
                new[] { typeof(JintRuntime), typeof(object), typeof(JsObject), typeof(object[]) }
            );

            var argumentsLocal = (BoundVariable)function.Body.Locals.SingleOrDefault(p => p.Name == "arguments");
            if (argumentsLocal == null)
            {
                Debug.Assert(function.Body.Closure != null);
                argumentsLocal = function.Body.Closure.Fields[Expressions.Closure.ArgumentsFieldName];
            }

            _scope = new Scope(
                new ILBuilder(methodBuilder.GetILGenerator(), _document),
                true,
                typeBuilder == null,
                function.Body.ScopedClosure,
                argumentsLocal,
                _scope
            );

            _scope.EmitLocals(function.Body.TypeManager);

            // Initialize our closure.

            var usedClosures = function.Body.TypeManager.UsedClosures;

            // Instantiate the closure if we own it.
            if (function.Body.Closure != null)
                EmitClosureSetup(function);

            // Emit locals for all used closures.
            if (function.Body.ScopedClosure != null && usedClosures != null)
                EmitClosureLocals(_closures[function.Body.ScopedClosure], usedClosures);

            // Put the closure local onto the stack if we're going to store
            // the arguments in the closure.
            if (argumentsLocal.Kind == BoundVariableKind.ClosureField)
                _scope.EmitLoadClosure(_closures[_scope.Closure].Type);

            // Initialize the arguments array.

            _scope.EmitLoad(SpecialLocal.Runtime);
            _scope.EmitLoad(SpecialLocal.Callee);
            _scope.EmitLoad(SpecialLocal.Arguments);

            IL.EmitCall(_runtimeCreateArguments);

            if (argumentsLocal.Kind == BoundVariableKind.ClosureField)
            {
                var argumentsClosureField = (BoundClosureField)argumentsLocal;

                IL.Emit(OpCodes.Stfld, _closures[function.Body.ScopedClosure].Fields[argumentsClosureField.Name]);
            }
            else
            {
                IL.Emit(OpCodes.Stloc, _scope.GetLocal(argumentsLocal.Type));
            }

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

            return methodBuilder;
        }

        private void EmitClosureSetup(BoundFunction function)
        {
            Debug.Assert(function.Body.Closure != null);

            var closure = _closures[function.Body.Closure];
            var constructor = closure.Constructor;

            // Check whether the constructor expects a parent.
            if (function.Body.Closure.Parent != null)
                IL.Emit(OpCodes.Ldarg_0);

            // Instantiate the closure.
            IL.Emit(OpCodes.Newobj, constructor);

            // Store the reference to the closure in the scope.

            var closureLocal = IL.DeclareLocal(closure.Type);
            _scope.RegisterClosureLocal(closureLocal);
            IL.Emit(OpCodes.Stloc, closureLocal);
        }

        private void EmitClosureLocals(RuntimeClosure closure, ReadOnlyArray<BoundClosure> usedClosures)
        {
            // If we don't have a parent field, there is no work.

            if (closure.Closure.Parent == null)
                return;

            var usage = new Dictionary<Type, ClosureUsage>();

            var parentUsage = DetermineClosureUsage(usage, _closures[closure.Closure.Parent], usedClosures);

            // If the parent isn't used, there is no work.

            if (parentUsage == ClosureUsage.NotUsed)
                return;

            // Emit all locals. We put the reference to the closure local on the
            // stack so the parent can be loaded from it.

            _scope.EmitLoadClosure(closure.Type);

            while (closure.Closure.Parent != null)
            {
                // Put the parent field on the stack. Above a check has already
                // been done to make sure at least one of the parent closures is
                // used, so we know that this is going to be popped.

                var parentField = closure.Fields[Expressions.Closure.ParentFieldName];
                IL.Emit(OpCodes.Ldfld, parentField);

                var parentClosure = _closures[closure.Closure.Parent];
                switch (usage[parentClosure.Type])
                {
                    case ClosureUsage.Directly:
                        // Declare the local for the closure and put it in the map.

                        var thisLocal = IL.DeclareLocal(parentField.FieldType);
                        _scope.RegisterClosureLocal(thisLocal);

                        bool isLast =
                            parentClosure.Closure.Parent == null ||
                            usage[_closures[parentClosure.Closure.Parent].Type] == ClosureUsage.NotUsed;

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

                closure = _closures[closure.Closure.Parent];
            }

            Debug.Fail("We shouldn't get here because all usages should have been marked");
        }

        private ClosureUsage DetermineClosureUsage(Dictionary<Type, ClosureUsage> usage, RuntimeClosure runtimeClosure, ReadOnlyArray<BoundClosure> usedClosures)
        {
            // Determine the usage of the parent.

            var parentUsage = ClosureUsage.NotUsed;

            FieldInfo parentField;
            if (runtimeClosure.Fields.TryGetValue(Expressions.Closure.ParentFieldName, out parentField))
                parentUsage = DetermineClosureUsage(usage, _closures[runtimeClosure.Closure.Parent], usedClosures);

            // Determine whether this closure is used.

            var used = ClosureUsage.NotUsed;

            foreach (var closure in usedClosures)
            {
                if (_closures[closure] == runtimeClosure)
                {
                    used = ClosureUsage.Directly;
                    break;
                }
            }

            // This closure is used indirectly if the parent is used.

            if (used != ClosureUsage.Directly && parentUsage != ClosureUsage.Indirectly)
                used = ClosureUsage.Indirectly;

            usage[runtimeClosure.Type] = used;

            return used;
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
                    var closure = closureField.Closure;

                    // Push the closure onto the stack.
                    _scope.EmitLoadClosure(_closures[closure].Type);
                    // Load the closure field.
                    IL.Emit(OpCodes.Ldfld, _closures[closure].Fields[closureField.Name]);

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

        private enum ClosureUsage
        {
            NotUsed,
            Directly,
            Indirectly
        }

        private class RuntimeClosure
        {
            public BoundClosure Closure { get; private set; }
            public TypeBuilder Type { get; private set; }
            public ConstructorBuilder Constructor { get; private set; }
            public Dictionary<string, FieldInfo> Fields { get; private set; }

            public RuntimeClosure(BoundClosure closure, TypeBuilder type, ConstructorBuilder constructor, Dictionary<string, FieldInfo> fields)
            {
                Closure = closure;
                Fields = fields;
                Type = type;
                Constructor = constructor;
            }
        }
    }
}
