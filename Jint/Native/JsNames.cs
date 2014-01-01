using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    internal static class JsNames
    {
        public static long Offset1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
        public static int TicksFactor = 10000;

        public static string DateTimeFormat = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'zzz";
        public static string DateTimeFormatUtc = "ddd, dd MMM yyyy HH':'mm':'ss 'UTC'";
        public static string DateFormat = "ddd, dd MMM yyyy";
        public static string TimeFormat = "HH':'mm':'ss 'GMT'zzz";

        public const string This = "this";
        public const string Arguments = "arguments";
        public const string Eval = "eval";

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
