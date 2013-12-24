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
