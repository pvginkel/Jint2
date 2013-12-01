using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal static class JsNames
    {
        public const int PrototypeId = -1;
        public const int ProtoId = -2;
        public const int ToStringId = -3;
        public const int ValueOfId = -4;
        public const int ValueId = -5;
        public const int WritableId = -6;
        public const int SetId = -7;
        public const int GetId = -8;
        public const int EnumerableId = -9;
        public const int ConfigurableId = -10;

        public const string This = "this";
        public const string Arguments = "arguments";
        public const string Call = "call";
        public const string Apply = "apply";
        public const string Constructor = "constructor";
        public const string Prototype = "prototype";
        public const string Proto = "__proto__";
        public const string Callee = "callee";
        public const string Length = "length";

        public const string TypeObject = "object";
        public const string TypeBoolean = "boolean";
        public const string TypeString = "string";
        public const string TypeNumber = "number";
        public const string TypeUndefined = "undefined";
        public const string TypeNull = "null";

        public const string TypeDescriptor = "descriptor";

        public const string TypeFunction = "function"; // used only in typeof operator!!!

        // embed classes ecma262.3 15

        public const string ClassNumber = "Number";
        public const string ClassString = "String";
        public const string ClassBoolean = "Boolean";

        public const string ClassObject = "Object";
        public const string ClassFunction = "Function";
        public const string ClassArray = "Array";
        public const string ClassRegexp = "RegExp";
        public const string ClassDate = "Date";
        public const string ClassError = "Error";

        public const string ClassArguments = "Arguments";
        public const string ClassGlobal = "Global";
        public const string ClassDescriptor = "Descriptor";
        public const string ClassScope = "Scope";
    }
}
