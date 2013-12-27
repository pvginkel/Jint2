using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Jint.Native;

namespace Jint.Bound
{
    partial class CodeGenerator
    {
        private static readonly MethodInfo _objectGetProperty = typeof(JsObject).GetMethod("GetProperty", new[] { typeof(int) }, null);
        private static readonly MethodInfo _objectGetPropertyCached = typeof(JsObject).GetMethod("GetProperty", new[] { typeof(int), typeof(DictionaryCacheSlot).MakeByRefType() }, null);
        private static readonly MethodInfo _objectSetProperty = typeof(JsObject).GetMethod("SetProperty", new[] { typeof(int), typeof(object) }, null);
        private static readonly MethodInfo _objectSetPropertyCached = typeof(JsObject).GetMethod("SetProperty", new[] { typeof(int), typeof(object), typeof(DictionaryCacheSlot).MakeByRefType() }, null);
        private static readonly MethodInfo _runtimeGetMemberByIndex = typeof(JintRuntime).GetMethod("GetMemberByIndex");
        private static readonly MethodInfo _runtimeSetMemberByIndex = typeof(JintRuntime).GetMethod("SetMemberByIndex");
        private static readonly MethodInfo _concat = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
        private static readonly MethodInfo _substring = typeof(string).GetMethod("Substring", new[] { typeof(int), typeof(int) });
        private static readonly MethodInfo _deleteByString = typeof(JsObject).GetMethod("DeleteProperty", new[] { typeof(string) });
        private static readonly MethodInfo _deleteByInstance = typeof(JsObject).GetMethod("DeleteProperty", new[] { typeof(object) });
        private static readonly MethodInfo _stringEquals = typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(string) });
        private static readonly MethodInfo _runtimeGetGlobal = typeof(JintRuntime).GetProperty("Global").GetGetMethod();
        private static readonly MethodInfo _runtimeGetGlobalScope = typeof(JintRuntime).GetProperty("GlobalScope").GetGetMethod();
        private static readonly MethodInfo _runtimeCreateFunction = typeof(JintRuntime).GetMethod("CreateFunction");
        private static readonly MethodInfo _runtimeCreateArguments = typeof(JintRuntime).GetMethod("CreateArguments");
        private static readonly MethodInfo _toBoolean = typeof(JsValue).GetMethod("ToBoolean");
        private static readonly MethodInfo _toNumber = typeof(JsValue).GetMethod("ToNumber");
        private static readonly MethodInfo _toString = typeof(JsValue).GetMethod("ToString", new[] { typeof(object) });
        private static readonly MethodInfo _numberToBoolean = typeof(JsConvert).GetMethod("ToBoolean", new[] { typeof(double) });
        private static readonly MethodInfo _numberToString = typeof(JsConvert).GetMethod("ToString", new[] { typeof(double) });
        private static readonly MethodInfo _stringToBoolean = typeof(JsConvert).GetMethod("ToBoolean", new[] { typeof(string) });
        private static readonly MethodInfo _stringToNumber = typeof(JsConvert).GetMethod("ToNumber", new[] { typeof(string) });
        private static readonly MethodInfo _booleanToNumber = typeof(JsConvert).GetMethod("ToNumber", new[] { typeof(bool) });
        private static readonly MethodInfo _booleanToString = typeof(JsConvert).GetMethod("ToString", new[] { typeof(bool) });
        private static readonly MethodInfo _objectExecute = typeof(JsObject).GetMethod("Execute");
        private static readonly MethodInfo _runtimeNew = typeof(JintRuntime).GetMethod("New");
        private static readonly MethodInfo _globalCreateArray = typeof(JsGlobal).GetMethod("CreateArray");
        private static readonly MethodInfo _globalCreateObject = typeof(JsGlobal).GetMethod("CreateObject", Type.EmptyTypes);
        private static readonly MethodInfo _objectDefineAccessor = typeof(JsObject).GetMethod("DefineAccessor", new[] { typeof(int), typeof(JsObject), typeof(JsObject) });
        private static readonly MethodInfo _runtimeWrapException = typeof(JintRuntime).GetMethod("WrapException");
        private static readonly MethodInfo _runtimeGetForEachKeys = typeof(JintRuntime).GetMethod("GetForEachKeys");
        private static readonly MethodInfo _globalCreateRegExp = typeof(JsGlobal).GetMethod("CreateRegExp", new[] { typeof(string), typeof(string) });
        private static readonly MethodInfo _objectHasProperty = typeof(JsObject).GetMethod("HasProperty", new[] { typeof(int) });
        private static readonly MethodInfo _runtimeHasMemberByIndex = typeof(JintRuntime).GetMethod("HasMemberByIndex");

        private static readonly ConstructorInfo _genericArgumentsConstructor = typeof(JsGenericArguments).GetConstructor(new[] { typeof(object[]) });
        private static readonly ConstructorInfo _functionConstructor = typeof(JsFunction).GetConstructors()[0];
        private static readonly ConstructorInfo _exceptionConstructor = typeof(JsException).GetConstructor(new[] { typeof(object) });

        private static readonly FieldInfo _nullInstance = typeof(JsNull).GetField("Instance");
        private static readonly FieldInfo _undefinedInstance = typeof(JsUndefined).GetField("Instance");
        private static readonly FieldInfo _emptyObjectArray = typeof(JsValue).GetField("EmptyArray");
    }
}
