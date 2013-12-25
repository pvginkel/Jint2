using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Jint.Bound;
using Jint.Native;
using Jint.Support;

namespace Jint.Compiler
{
    internal class TypeSystem
    {
#if DEBUG || SAVE_ASSEMBLY
        private static int _lastAssemblyId;
#endif
        private AssemblyBuilder _assemblyBuilder;
        private int _lastTypeId;
        private int _lastClosureId;
        private ModuleBuilder _moduleBuilder;

        public bool SaveAssembly { get; set; }

        public TypeSystem()
        {
#if DEBUG || SAVE_ASSEMBLY
            SaveAssembly = true;
#endif

            CreateAssemblyBuilder();
        }

        private void CreateAssemblyBuilder()
        {
            string name = "DynamicJintAssembly";
            var assemblyAccess = AssemblyBuilderAccess.RunAndCollect;

#if DEBUG || SAVE_ASSEMBLY
            int assemblyId = Interlocked.Increment(ref _lastAssemblyId);
            name += assemblyId.ToString(CultureInfo.InvariantCulture);

            if (SaveAssembly)
                assemblyAccess = AssemblyBuilderAccess.RunAndSave;
#endif

            var assemblyName = new AssemblyName(name);

            _assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                assemblyName,
                assemblyAccess
            );

            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(
                assemblyName.Name,
                assemblyName.Name + ".dll",
                true
            );
        }

        private void FlushAssembly()
        {
#if DEBUG || SAVE_ASSEMBLY
            if (SaveAssembly)
            {
                _assemblyBuilder.Save(_assemblyBuilder.GetName().Name + ".dll");

                CreateAssemblyBuilder();
            }
#endif
        }

        public IScriptBuilder CreateScriptBuilder(string fileName)
        {
            int typeId = Interlocked.Increment(ref _lastTypeId);

            ISymbolDocumentWriter document = null;

            if (fileName != null)
            {
                document = _moduleBuilder.DefineDocument(
                    Path.GetFullPath(fileName),
                    SymLanguageType.JScript,
                    SymLanguageVendor.Microsoft,
                    SymDocumentType.Text
                    );
            }

            return new ScriptBuilder(
                fileName,
                _moduleBuilder.DefineType(
                    "CompiledExpression" + typeId.ToString(CultureInfo.InvariantCulture),
                    TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed
                    // TypeAttributes.Abstract | TypeAttributes.Sealed equals static.
                ),
                document,
                this
            );
        }

        private abstract class TypeBuilder : ITypeBuilder
        {
            private readonly List<IFunctionBuilder> _functions = new List<IFunctionBuilder>();
            private readonly HashSet<string> _compiledMethodNames = new HashSet<string>();
            private int _nextMethodId;

            public Type Type { get; private set; }
            protected TypeSystem TypeSystem { get; private set; }
            public ISymbolDocumentWriter Document { get; private set; }
            public IList<IFunctionBuilder> Functions { get; private set; }

            protected TypeBuilder(System.Reflection.Emit.TypeBuilder type, TypeSystem typeSystem, ISymbolDocumentWriter document)
            {
                Type = type;
                TypeSystem = typeSystem;
                Document = document;
                Functions = new ReadOnlyCollection<IFunctionBuilder>(_functions);
            }

            public abstract IFunctionBuilder CreateFunction(Type delegateType, string name);

            protected IFunctionBuilder CreateFunction(Type delegateType, string name, MethodAttributes attributes)
            {
                var invoke = delegateType.GetMethod("Invoke");

                var result = new FunctionBuilder(
                    this,
                    ((System.Reflection.Emit.TypeBuilder)Type).DefineMethod(
                        GetFunctionName(name),
                        attributes,
                        invoke.ReturnType,
                        invoke.GetParameters().Select(p => p.ParameterType).ToArray()
                    )
                );

                _functions.Add(result);

                return result;
            }

            private string GetFunctionName(string name)
            {
                if (name == null)
                {
                    int id = _nextMethodId++;

                    if (id == 0)
                        name = "(anonymous function)";
                    else
                        name = "(anonymous function " + id.ToString(CultureInfo.InvariantCulture) + ")";
                }
                else
                {
                    string proposal = name;

                    for (int i = 0; ; i++)
                    {
                        if (i > 0)
                            proposal = name + "$" + i.ToString(CultureInfo.InvariantCulture);

                        if (!_compiledMethodNames.Contains(proposal))
                        {
                            name = proposal;
                            break;
                        }
                    }
                }

                _compiledMethodNames.Add(name);

                return name;
            }

            public virtual void Commit()
            {
                Type = ((System.Reflection.Emit.TypeBuilder)Type).CreateType();

                foreach (FunctionBuilder function in _functions)
                {
                    function.Method = Type.GetMethod(function.Method.Name);
                }
            }
        }

        private class ScriptBuilder : TypeBuilder, IScriptBuilder
        {
            private readonly List<IClosureBuilder> _closures = new List<IClosureBuilder>();

            public string FileName { get; private set; }
            public IList<IClosureBuilder> Closures { get; private set; }

            public ScriptBuilder(string fileName, System.Reflection.Emit.TypeBuilder type, ISymbolDocumentWriter document, TypeSystem typeSystem)
                : base(type, typeSystem, document)
            {
                FileName = fileName;
                Closures = new ReadOnlyCollection<IClosureBuilder>(_closures);
            }

            public override IFunctionBuilder CreateFunction(Type delegateType, string name)
            {
                return CreateFunction(delegateType, name, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);
            }

            public override void Commit()
            {
                // Create the type for ourselves and all closures.

                foreach (var closure in Closures)
                {
                    ((TypeBuilder)closure).Commit();
                }

                base.Commit();

                // We've build a complete script. Dump the assembly (with the right
                // constants defined) so the generated assembly can be inspected.

                TypeSystem.FlushAssembly();
            }

            public void CommitClosureFields()
            {
                foreach (ClosureBuilder closure in Closures)
                {
                    closure.CommitFields();
                }
            }

            public IClosureBuilder CreateClosureBuilder(BoundClosure closure)
            {
                if (closure == null)
                    throw new ArgumentNullException("closure");

                int id = Interlocked.Increment(ref TypeSystem._lastClosureId);

                var type = TypeSystem._moduleBuilder.DefineType(
                    "<>JintClosure_" + id.ToString(CultureInfo.InvariantCulture),
                    TypeAttributes.NotPublic | TypeAttributes.Sealed
                );

                type.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(CompilerGeneratedAttribute).GetConstructors()[0],
                    new object[0]
                ));

                var result = new ClosureBuilder(closure, TypeSystem, type, Document);

                _closures.Add(result);

                return result;
            }
        }

        private class FunctionBuilder : IFunctionBuilder
        {
            private ILBuilder _ilBuilder;

            public ITypeBuilder TypeBuilder { get; private set; }
            public MethodInfo Method { get; set; }

            public ILBuilder GetILBuilder()
            {
                if (_ilBuilder == null)
                    _ilBuilder = new ILBuilder(((MethodBuilder)Method).GetILGenerator(), ((TypeBuilder)TypeBuilder).Document);

                return _ilBuilder;
            }

            public FunctionBuilder(ITypeBuilder typeBuilder, MethodBuilder method)
            {
                TypeBuilder = typeBuilder;
                Method = method;
            }
        }

        private class ClosureBuilder : TypeBuilder, IClosureBuilder
        {
            private const string ParentFieldName = "<>parent";

            private static readonly ConstructorInfo _objectConstructor = typeof(object).GetConstructors()[0];
            private static readonly FieldInfo _undefinedInstance = typeof(JsUndefined).GetField("Instance");

            private readonly ClosureParentFieldCollection _parentFields = new ClosureParentFieldCollection();

            public BoundClosure Closure { get; private set; }
            public ConstructorBuilder Constructor { get; private set; }
            public IKeyedCollection<IClosureBuilder, ClosureParentField> ParentFields { get; private set; }

            public ClosureBuilder(BoundClosure closure, TypeSystem typeSystem, System.Reflection.Emit.TypeBuilder type, ISymbolDocumentWriter document)
                : base(type, typeSystem, document)
            {
                Closure = closure;
                ParentFields = ReadOnlyKeyedCollection.Create(_parentFields);
            }

            public override IFunctionBuilder CreateFunction(Type delegateType, string name)
            {
                return CreateFunction(delegateType, name, MethodAttributes.Public | MethodAttributes.HideBySig);
            }

            public IClosureFieldBuilder CreateClosureFieldBuilder(BoundClosureField field)
            {
                if (field == null)
                    throw new ArgumentNullException("field");

                return new ClosureFieldBuilder(field);
            }

            public void CommitFields()
            {
                Debug.Assert(Constructor == null);

                var typeBuilder = (System.Reflection.Emit.TypeBuilder)Type;

                int index = 1;
                var parent = Closure.Parent;

                while (parent != null)
                {
                    var field = typeBuilder.DefineField(
                        ParentFieldName + "$" + index.ToString(CultureInfo.InvariantCulture),
                        parent.Builder.Type,
                        FieldAttributes.Public | FieldAttributes.InitOnly
                    );

                    _parentFields.Add(new ClosureParentField(parent.Builder, field));

                    index++;

                    parent = parent.Parent;
                }

                foreach (var field in Closure.Fields.OrderBy(p => p.Name))
                {
                    ((ClosureFieldBuilder)field.Builder).Field = typeBuilder.DefineField(
                        field.Name,
                        field.ValueType.GetNativeType(),
                        FieldAttributes.Public
                    );
                }

                Constructor = EmitClosureConstructor();
            }

            private ConstructorBuilder EmitClosureConstructor()
            {
                var parameterTypes = Type.EmptyTypes;

                if (Closure.Parent != null)
                    parameterTypes = new[] { Closure.Parent.Builder.Type };

                var constructor = ((System.Reflection.Emit.TypeBuilder)Type).DefineConstructor(
                    MethodAttributes.Public,
                    CallingConventions.Standard,
                    parameterTypes
                );

                var il = new ILBuilder(constructor.GetILGenerator(), null);

                // Call the base constructor.

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, _objectConstructor);

                // Initialize all parent fields.

                if (Closure.Parent != null)
                {
                    // Load this.
                    il.Emit(OpCodes.Ldarg_0);
                    // Load the parent from the arguments.
                    il.Emit(OpCodes.Ldarg_1);
                    // Store in the parent field.
                    il.Emit(OpCodes.Stfld, _parentFields[Closure.Parent.Builder].Field);

                    var parent = Closure.Parent.Parent;

                    while (parent != null)
                    {
                        // Load the this (used for the store).
                        il.Emit(OpCodes.Ldarg_0);

                        // Load the parent from the arguments.
                        il.Emit(OpCodes.Ldarg_1);
                        // Load correct parent from the parent.
                        il.Emit(OpCodes.Ldfld, Closure.Parent.Builder.ParentFields[parent.Builder].Field);

                        // Store the parent into our field.
                        il.Emit(OpCodes.Stfld, _parentFields[parent.Builder].Field);

                        parent = parent.Parent;
                    }
                }

                // Initialize object fields to undefined.

                foreach (var field in Closure.Fields)
                {
                    if (field.Builder.Field.FieldType == typeof(object))
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldsfld, _undefinedInstance);
                        il.Emit(OpCodes.Stfld, field.Builder.Field);
                    }
                }

                // Return from the constructor.

                il.Emit(OpCodes.Ret);

                return constructor;
            }

            private  class ClosureParentFieldCollection : Support.KeyedCollection<IClosureBuilder, ClosureParentField>
            {
                protected override IClosureBuilder GetKeyForItem(ClosureParentField item)
                {
                    return item.Closure;
                }
            }
        }

        private class ClosureFieldBuilder : IClosureFieldBuilder
        {
            public BoundClosureField ClosureField { get; private set; }
            public FieldBuilder Field { get; set; }

            public ClosureFieldBuilder(BoundClosureField closureField)
            {
                if (closureField == null)
                    throw new ArgumentNullException("closureField");

                ClosureField = closureField;
            }
        }
    }
}
