using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jint.Backend.Dlr
{
    internal static class ClosureManager
    {
        private static int _lastId;
        private static readonly AssemblyBuilder _dynamicAssembly;
        private static readonly ModuleBuilder _dynamicModule;

        static ClosureManager()
        {
            _dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("JintClosures"),
                AssemblyBuilderAccess.Run,
                null,
                true /* isSynchronized */,
                null
            );

            _dynamicModule = _dynamicAssembly.DefineDynamicModule("JintClosuresModule");
        }

        public static Type BuildClosure(Dictionary<string, Type> fields)
        {
            if (fields == null)
                throw new ArgumentNullException("fields");

            int id = Interlocked.Increment(ref _lastId);

            var dynamicType = _dynamicModule.DefineType(
                "<>JintClosure_" + id.ToString(CultureInfo.InvariantCulture),
                TypeAttributes.SpecialName
            );

            foreach (var field in fields.OrderBy(p => p.Key))
            {
                dynamicType.DefineField(field.Key, field.Value, FieldAttributes.Public);
            }

            return dynamicType.CreateType();
        }
    }
}
