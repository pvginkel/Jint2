using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private void BuildEnvironment()
        {
            var objectPrototype = CreateObject(PrototypeSink);
            var functionPrototype = CreateFunction(null, FunctionFunctions.BaseConstructor, 0, null, objectPrototype);

            // These two must be initialized special because they depend on
            // each other being available.

            FunctionClass = BuildFunctionClass(functionPrototype);
            InitializeFunctionClass();

            ObjectClass = BuildObjectClass(objectPrototype);
            ArrayClass = BuildArrayClass();
            BooleanClass = BuildBooleanClass();
            DateClass = BuildDateClass();
            NumberClass = BuildNumberClass();
            RegExpClass = BuildRegExpClass();
            StringClass = BuildStringClass();
            MathClass = BuildMathClass();

            ErrorClass = BuildErrorClass("Error");
            EvalErrorClass = BuildErrorClass("EvalError");
            RangeErrorClass = BuildErrorClass("RangeError");
            ReferenceErrorClass = BuildErrorClass("ReferenceError");
            SyntaxErrorClass = BuildErrorClass("SyntaxError");
            TypeErrorClass = BuildErrorClass("TypeError");
            URIErrorClass = BuildErrorClass("URIError");
        }

        private JsObject BuildObjectClass(JsObject prototype)
        {
            // We need to keep this because the prototype is passed to the constructor rather than created in it.
            DefineFunction(prototype, "constructor", ObjectFunctions.GetConstructor, 0, PropertyAttributes.DontEnum);

            DefineFunction(prototype, "toString", ObjectFunctions.ToString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toLocaleString", ObjectFunctions.ToString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "valueOf", ObjectFunctions.ValueOf, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "hasOwnProperty", ObjectFunctions.HasOwnProperty, 1, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "isPrototypeOf", ObjectFunctions.IsPrototypeOf, 1, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "propertyIsEnumerable", ObjectFunctions.PropertyIsEnumerable, 1, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getPrototypeOf", ObjectFunctions.GetPrototypeOf, 1, PropertyAttributes.DontEnum);

            if (HasOption(Options.EcmaScript5))
            {
                DefineFunction(prototype, "defineProperty", ObjectFunctions.DefineProperty, 1, PropertyAttributes.DontEnum);
                DefineFunction(prototype, "__lookupGetter__", ObjectFunctions.LookupGetter, 1, PropertyAttributes.DontEnum);
                DefineFunction(prototype, "__lookupSetter__", ObjectFunctions.LookupSetter, 1, PropertyAttributes.DontEnum);
            }

            return CreateFunction("Object", ObjectFunctions.Constructor, 0, null, prototype);
        }

        private JsObject BuildFunctionClass(JsObject functionPrototype)
        {
            // Direct call to the JsFunction constructor because of bootstrapping.

            var result = CreateObject(
                null,
                functionPrototype,
                new JsDelegate(
                    "Function",
                    FunctionFunctions.Constructor,
                    0,
                    null
                )
            );

            result.SetClass(JsNames.ClassFunction);
            result.SetIsClr(false);

            return result;
        }

        private void InitializeFunctionClass()
        {
            var prototype = FunctionClass.Prototype;

            DefineFunction(prototype, "constructor", FunctionFunctions.GetConstructor, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "call", FunctionFunctions.Call, 1, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "apply", FunctionFunctions.Apply, 2, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toString", FunctionFunctions.ToString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toLocaleString", FunctionFunctions.ToString, 0, PropertyAttributes.DontEnum);
            DefineProperty(prototype, "length", FunctionFunctions.GetLength, FunctionFunctions.SetLength, PropertyAttributes.DontEnum);
        }

        private JsObject BuildArrayClass()
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            DefineProperty(prototype, "length", ArrayFunctions.GetLength, ArrayFunctions.SetLength, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toString", ArrayFunctions.ToString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toLocaleString", ArrayFunctions.ToLocaleString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "concat", ArrayFunctions.Concat, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "join", ArrayFunctions.Join, 1, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "pop", ArrayFunctions.Pop, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "push", ArrayFunctions.Push, 1, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "reverse", ArrayFunctions.Reverse, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "shift", ArrayFunctions.Shift, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "slice", ArrayFunctions.Slice, 2, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "sort", ArrayFunctions.Sort, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "splice", ArrayFunctions.Splice, 2, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "unshift", ArrayFunctions.UnShift, 1, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "indexOf", ArrayFunctions.IndexOf, 1, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "lastIndexOf", ArrayFunctions.LastIndexOf, 1, PropertyAttributes.DontEnum);

            return CreateFunction("Array", ArrayFunctions.Constructor, 0, null, prototype);
        }

        private JsObject BuildBooleanClass()
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            DefineFunction(prototype, "toString", BooleanFunctions.ToString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toLocaleString", BooleanFunctions.ToString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "valueOf", BooleanFunctions.ValueOf, 0, PropertyAttributes.DontEnum);

            return CreateFunction("Boolean", BooleanFunctions.Constructor, 0, null, prototype);
        }

        private JsObject BuildDateClass()
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            DefineFunction(prototype, "toString", DateFunctions.ToString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toDateString", DateFunctions.ToDateString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toTimeString", DateFunctions.ToTimeString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toLocaleString", DateFunctions.ToLocaleString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toLocaleDateString", DateFunctions.ToLocaleDateString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toLocaleTimeString", DateFunctions.ToLocaleTimeString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "valueOf", DateFunctions.ValueOf, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getTime", DateFunctions.GetTime, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getFullYear", DateFunctions.GetFullYear, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getUTCFullYear", DateFunctions.GetUTCFullYear, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getMonth", DateFunctions.GetMonth, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getUTCMonth", DateFunctions.GetUTCMonth, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getDate", DateFunctions.GetDate, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getUTCDate", DateFunctions.GetUTCDate, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getDay", DateFunctions.GetDay, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getUTCDay", DateFunctions.GetUTCDay, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getHours", DateFunctions.GetHours, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getUTCHours", DateFunctions.GetUTCHours, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getMinutes", DateFunctions.GetMinutes, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getUTCMinutes", DateFunctions.GetUTCMinutes, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getSeconds", DateFunctions.GetSeconds, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getUTCSeconds", DateFunctions.GetUTCSeconds, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getMilliseconds", DateFunctions.GetMilliseconds, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getUTCMilliseconds", DateFunctions.GetUTCMilliseconds, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "getTimezoneOffset", DateFunctions.GetTimezoneOffset, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "setTime", DateFunctions.SetTime, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "setMilliseconds", DateFunctions.SetMilliseconds, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "setUTCMilliseconds", DateFunctions.SetUTCMilliseconds, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "setSeconds", DateFunctions.SetSeconds, 2, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "setUTCSeconds", DateFunctions.SetUTCSeconds, 2, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "setMinutes", DateFunctions.SetMinutes, 3, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "setUTCMinutes", DateFunctions.SetUTCMinutes, 3, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "setHours", DateFunctions.SetHours, 4, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "setUTCHours", DateFunctions.SetUTCHours, 4, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "setDate", DateFunctions.SetDate, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "setUTCDate", DateFunctions.SetUTCDate, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "setMonth", DateFunctions.SetMonth, 2, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "setUTCMonth", DateFunctions.SetUTCMonth, 2, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "setFullYear", DateFunctions.SetFullYear, 3, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "setUTCFullYear", DateFunctions.SetUTCFullYear, 3, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toUTCString", DateFunctions.ToUTCString, 0, PropertyAttributes.DontEnum);

            var result = CreateFunction("Date", DateFunctions.Constructor, 0, null, prototype);

            DefineProperty(result, "now", DateFunctions.Now, null, PropertyAttributes.DontEnum);
            DefineFunction(result, "parse", DateFunctions.Parse, 1, PropertyAttributes.DontEnum);
            DefineFunction(result, "parseLocale", DateFunctions.ParseLocale, 1, PropertyAttributes.DontEnum);
            DefineFunction(result, "UTC", DateFunctions.UTC, 1, PropertyAttributes.DontEnum);

            return result;
        }

        private JsObject BuildNumberClass()
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            DefineFunction(prototype, "toString", NumberFunctions.ToString, 1, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toLocaleString", NumberFunctions.ToLocaleString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toFixed", NumberFunctions.ToFixed, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toExponential", NumberFunctions.ToExponential, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toPrecision", NumberFunctions.ToPrecision, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "valueOf", NumberFunctions.ValueOf, 0, PropertyAttributes.DontEnum);

            var result = CreateFunction("Number", NumberFunctions.Constructor, 0, null, prototype);

            DefineProperty(result, "MAX_VALUE", JsBox.MaxValue, PropertyAttributes.None);
            DefineProperty(result, "MIN_VALUE", JsBox.MinValue, PropertyAttributes.None);
            DefineProperty(result, "NaN", JsBox.NaN, PropertyAttributes.None);
            DefineProperty(result, "POSITIVE_INFINITY", JsBox.PositiveInfinity, PropertyAttributes.None);
            DefineProperty(result, "NEGATIVE_INFINITY", JsBox.NegativeInfinity, PropertyAttributes.None);

            return result;
        }

        private JsObject BuildRegExpClass()
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            DefineFunction(prototype, "toString", RegExpFunctions.ToString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toLocaleString", RegExpFunctions.ToString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "lastIndex", RegExpFunctions.GetLastIndex, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "exec", RegExpFunctions.Exec, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "test", RegExpFunctions.Test, 0, PropertyAttributes.DontEnum);

            return CreateFunction("RegExp", RegExpFunctions.Constructor, 0, null, prototype);
        }

        private JsObject BuildStringClass()
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            DefineFunction(prototype, "split", StringFunctions.Split, 2, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "replace", StringFunctions.Replace, 2, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toString", StringFunctions.ToString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toLocaleString", StringFunctions.ToString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "match", StringFunctions.Match, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "localeCompare", StringFunctions.LocaleCompare, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "substring", StringFunctions.Substring, 2, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "substr", StringFunctions.Substr, 2, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "search", StringFunctions.Search, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "valueOf", StringFunctions.ValueOf, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "concat", StringFunctions.Concat, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "charAt", StringFunctions.CharAt, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "charCodeAt", StringFunctions.CharCodeAt, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "lastIndexOf", StringFunctions.LastIndexOf, 1, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "indexOf", StringFunctions.IndexOf, 1, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toLowerCase", StringFunctions.ToLowerCase, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toLocaleLowerCase", StringFunctions.ToLocaleLowerCase, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toUpperCase", StringFunctions.ToUpperCase, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toLocaleUpperCase", StringFunctions.ToLocaleUpperCase, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "slice", StringFunctions.Slice, 2, PropertyAttributes.DontEnum);
            DefineProperty(prototype, "length", StringFunctions.GetLength, null, PropertyAttributes.DontEnum);

            var result = CreateFunction("String", StringFunctions.Constructor, 0, null, prototype);

            DefineFunction(result, "fromCharCode", StringFunctions.FromCharCode, 1, PropertyAttributes.DontEnum);

            return result;
        }

        private JsObject BuildMathClass()
        {
            var result = CreateObject(ObjectClass.Prototype);

            DefineFunction(result, "abs", MathFunctions.Abs, 1, PropertyAttributes.DontEnum);
            DefineFunction(result, "acos", MathFunctions.Acos, 1, PropertyAttributes.DontEnum);
            DefineFunction(result, "asin", MathFunctions.Asin, 1, PropertyAttributes.DontEnum);
            DefineFunction(result, "atan", MathFunctions.Atan, 1, PropertyAttributes.DontEnum);
            DefineFunction(result, "atan2", MathFunctions.Atan2, 2, PropertyAttributes.DontEnum);
            DefineFunction(result, "ceil", MathFunctions.Ceil, 1, PropertyAttributes.DontEnum);
            DefineFunction(result, "cos", MathFunctions.Cos, 1, PropertyAttributes.DontEnum);
            DefineFunction(result, "exp", MathFunctions.Exp, 1, PropertyAttributes.DontEnum);
            DefineFunction(result, "floor", MathFunctions.Floor, 1, PropertyAttributes.DontEnum);
            DefineFunction(result, "log", MathFunctions.Log, 1, PropertyAttributes.DontEnum);
            DefineFunction(result, "max", MathFunctions.Max, 1, PropertyAttributes.DontEnum);
            DefineFunction(result, "min", MathFunctions.Min, 1, PropertyAttributes.DontEnum);
            DefineFunction(result, "pow", MathFunctions.Pow, 2, PropertyAttributes.DontEnum);
            DefineFunction(result, "random", MathFunctions.Random, 0, PropertyAttributes.DontEnum);
            DefineFunction(result, "round", MathFunctions.Round, 1, PropertyAttributes.DontEnum);
            DefineFunction(result, "sin", MathFunctions.Sin, 1, PropertyAttributes.DontEnum);
            DefineFunction(result, "sqrt", MathFunctions.Sqrt, 1, PropertyAttributes.DontEnum);
            DefineFunction(result, "tan", MathFunctions.Tan, 1, PropertyAttributes.DontEnum);
            DefineProperty(result, "E", JsNumber.Box(Math.E), PropertyAttributes.DontEnum);
            DefineProperty(result, "LN2", JsNumber.Box(Math.Log(2)), PropertyAttributes.DontEnum);
            DefineProperty(result, "LN10", JsNumber.Box(Math.Log(10)), PropertyAttributes.DontEnum);
            DefineProperty(result, "LOG2E", JsNumber.Box(Math.Log(Math.E, 2)), PropertyAttributes.DontEnum);
            DefineProperty(result, "PI", JsNumber.Box(Math.PI), PropertyAttributes.DontEnum);
            DefineProperty(result, "SQRT1_2", JsNumber.Box(Math.Sqrt(0.5)), PropertyAttributes.DontEnum);
            DefineProperty(result, "SQRT2", JsNumber.Box(Math.Sqrt(2)), PropertyAttributes.DontEnum);

            return result;
        }

        private JsObject BuildErrorClass(string name)
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            DefineProperty(prototype, "name", JsString.Box(name), PropertyAttributes.DontEnum | PropertyAttributes.DontDelete | PropertyAttributes.ReadOnly);
            DefineFunction(prototype, "toString", ErrorFunctions.ToString, 0, PropertyAttributes.DontEnum);
            DefineFunction(prototype, "toLocaleString", ErrorFunctions.ToString, 0, PropertyAttributes.DontEnum);

            return CreateFunction(name, ErrorFunctions.Constructor, 0, null, prototype);
        }
    }
}
