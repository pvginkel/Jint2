using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jint.Native;
using Jint.Support;

namespace Jint.Compiler
{
    internal static class DynamicAssemblyManager
    {
        private static readonly FieldInfo _undefinedInstance = typeof(JsUndefined).GetField("Instance");
        private static readonly ConstructorInfo _objectConstructor = typeof(object).GetConstructors()[0];

        private static int _lastId;
        private static AssemblyBuilder _assemblyBuilder;
#if DEBUG || SAVE_ASSEMBLY
        private static int _lastAssemblyId;
#endif
        public static bool SaveAssembly { get; set; }

        public static ModuleBuilder ModuleBuilder { get; private set; }
        public static DebugInfoGenerator PdbGenerator { get; private set; }

        static DynamicAssemblyManager()
        {
#if DEBUG || SAVE_ASSEMBLY
            SaveAssembly = true;
#endif
            CreateAssemblyBuilder();
        }

        private static void CreateAssemblyBuilder()
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

            PdbGenerator = DebugInfoGenerator.CreatePdbGenerator();

            ModuleBuilder = _assemblyBuilder.DefineDynamicModule(
                assemblyName.Name,
                assemblyName.Name + ".dll",
                true
            );
        }

        public static Type BuildClosure(Dictionary<string, Type> fields)
        {
            if (fields == null)
                throw new ArgumentNullException("fields");

            int id = Interlocked.Increment(ref _lastId);

            var dynamicType = ModuleBuilder.DefineType(
                "<>JintClosure_" + id.ToString(CultureInfo.InvariantCulture),
                TypeAttributes.SpecialName
            );

            var typeFields = new Dictionary<string, FieldBuilder>();

            foreach (var field in fields.OrderBy(p => p.Key))
            {
                var attributes = FieldAttributes.Public;
                if (field.Key == Expressions.Closure.ParentFieldName)
                    attributes |= FieldAttributes.InitOnly;

                typeFields.Add(
                    field.Key,
                    dynamicType.DefineField(field.Key, field.Value, attributes)
                );
            }

            var parameterTypes = Type.EmptyTypes;

            var parentField = fields.SingleOrDefault(p => p.Key == Expressions.Closure.ParentFieldName).Value;
            if (parentField != null)
                parameterTypes = new[] { parentField };

            var constructor = dynamicType.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                parameterTypes
            );

            var il = new ILBuilder(constructor.GetILGenerator(), null);

            // Call the base constructor.

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, _objectConstructor);

            // Initialize the parent field if we have one.

            if (parentField != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, typeFields[Expressions.Closure.ParentFieldName]);
            }

            // Initialize object fields to undefined.

            foreach (var field in typeFields)
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

            return dynamicType.CreateType();
        }

        public static void FlushAssembly()
        {
#if DEBUG || SAVE_ASSEMBLY
            if (SaveAssembly)
            {
                _assemblyBuilder.Save(_assemblyBuilder.GetName().Name + ".dll");

                CreateAssemblyBuilder();
            }
#endif
        }
    }
}
