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
            prototype.DefineProperty("toString", ObjectFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toLocaleString", ObjectFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("valueOf", ObjectFunctions.ValueOf, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("hasOwnProperty", ObjectFunctions.HasOwnProperty, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty("isPrototypeOf", ObjectFunctions.IsPrototypeOf, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty("propertyIsEnumerable", ObjectFunctions.PropertyIsEnumerable, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getPrototypeOf", ObjectFunctions.GetPrototypeOf, 1, PropertyAttributes.DontEnum);

            if (HasOption(Options.EcmaScript5))
            {
                prototype.DefineProperty("defineProperty", ObjectFunctions.DefineProperty, 1, PropertyAttributes.DontEnum);
                prototype.DefineProperty("__lookupGetter__", ObjectFunctions.LookupGetter, 1, PropertyAttributes.DontEnum);
                prototype.DefineProperty("__lookupSetter__", ObjectFunctions.LookupSetter, 1, PropertyAttributes.DontEnum);
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

            prototype.DefineProperty("constructor", JsBox.CreateObject(FunctionClass), PropertyAttributes.DontEnum);
            prototype.DefineProperty("call", FunctionFunctions.Call, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty("apply", FunctionFunctions.Apply, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toString", FunctionFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toLocaleString", FunctionFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineAccessor("length", FunctionFunctions.GetLength, FunctionFunctions.SetLength, PropertyAttributes.DontEnum);
        }

        private JsObject BuildArrayClass()
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            prototype.DefineAccessor("length", ArrayFunctions.GetLength, ArrayFunctions.SetLength, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toString", ArrayFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toLocaleString", ArrayFunctions.ToLocaleString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("concat", ArrayFunctions.Concat, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("join", ArrayFunctions.Join, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty("pop", ArrayFunctions.Pop, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("push", ArrayFunctions.Push, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty("reverse", ArrayFunctions.Reverse, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("shift", ArrayFunctions.Shift, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("slice", ArrayFunctions.Slice, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty("sort", ArrayFunctions.Sort, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("splice", ArrayFunctions.Splice, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty("unshift", ArrayFunctions.UnShift, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty("indexOf", ArrayFunctions.IndexOf, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty("lastIndexOf", ArrayFunctions.LastIndexOf, 1, PropertyAttributes.DontEnum);

            return CreateFunction("Array", ArrayFunctions.Constructor, 0, null, prototype);
        }

        private JsObject BuildBooleanClass()
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            prototype.DefineProperty("toString", BooleanFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toLocaleString", BooleanFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("valueOf", BooleanFunctions.ValueOf, 0, PropertyAttributes.DontEnum);

            return CreateFunction("Boolean", BooleanFunctions.Constructor, 0, null, prototype);
        }

        private JsObject BuildDateClass()
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            prototype.DefineProperty("toString", DateFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toDateString", DateFunctions.ToDateString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toTimeString", DateFunctions.ToTimeString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toLocaleString", DateFunctions.ToLocaleString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toLocaleDateString", DateFunctions.ToLocaleDateString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toLocaleTimeString", DateFunctions.ToLocaleTimeString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("valueOf", DateFunctions.ValueOf, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getTime", DateFunctions.GetTime, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getFullYear", DateFunctions.GetFullYear, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getUTCFullYear", DateFunctions.GetUTCFullYear, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getMonth", DateFunctions.GetMonth, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getUTCMonth", DateFunctions.GetUTCMonth, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getDate", DateFunctions.GetDate, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getUTCDate", DateFunctions.GetUTCDate, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getDay", DateFunctions.GetDay, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getUTCDay", DateFunctions.GetUTCDay, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getHours", DateFunctions.GetHours, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getUTCHours", DateFunctions.GetUTCHours, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getMinutes", DateFunctions.GetMinutes, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getUTCMinutes", DateFunctions.GetUTCMinutes, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getSeconds", DateFunctions.GetSeconds, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getUTCSeconds", DateFunctions.GetUTCSeconds, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getMilliseconds", DateFunctions.GetMilliseconds, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getUTCMilliseconds", DateFunctions.GetUTCMilliseconds, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("getTimezoneOffset", DateFunctions.GetTimezoneOffset, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("setTime", DateFunctions.SetTime, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("setMilliseconds", DateFunctions.SetMilliseconds, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("setUTCMilliseconds", DateFunctions.SetUTCMilliseconds, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("setSeconds", DateFunctions.SetSeconds, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty("setUTCSeconds", DateFunctions.SetUTCSeconds, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty("setMinutes", DateFunctions.SetMinutes, 3, PropertyAttributes.DontEnum);
            prototype.DefineProperty("setUTCMinutes", DateFunctions.SetUTCMinutes, 3, PropertyAttributes.DontEnum);
            prototype.DefineProperty("setHours", DateFunctions.SetHours, 4, PropertyAttributes.DontEnum);
            prototype.DefineProperty("setUTCHours", DateFunctions.SetUTCHours, 4, PropertyAttributes.DontEnum);
            prototype.DefineProperty("setDate", DateFunctions.SetDate, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("setUTCDate", DateFunctions.SetUTCDate, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("setMonth", DateFunctions.SetMonth, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty("setUTCMonth", DateFunctions.SetUTCMonth, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty("setFullYear", DateFunctions.SetFullYear, 3, PropertyAttributes.DontEnum);
            prototype.DefineProperty("setUTCFullYear", DateFunctions.SetUTCFullYear, 3, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toUTCString", DateFunctions.ToUTCString, 0, PropertyAttributes.DontEnum);

            var result = CreateFunction("Date", DateFunctions.Constructor, 0, null, prototype);

            result.DefineAccessor("now", DateFunctions.Now, null, PropertyAttributes.DontEnum);
            result.DefineProperty("parse", DateFunctions.Parse, 1, PropertyAttributes.DontEnum);
            result.DefineProperty("parseLocale", DateFunctions.ParseLocale, 1, PropertyAttributes.DontEnum);
            result.DefineProperty("UTC", DateFunctions.UTC, 1, PropertyAttributes.DontEnum);

            return result;
        }

        private JsObject BuildNumberClass()
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            prototype.DefineProperty("toString", NumberFunctions.ToString, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toLocaleString", NumberFunctions.ToLocaleString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toFixed", NumberFunctions.ToFixed, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toExponential", NumberFunctions.ToExponential, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toPrecision", NumberFunctions.ToPrecision, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("valueOf", NumberFunctions.ValueOf, 0, PropertyAttributes.DontEnum);

            var result = CreateFunction("Number", NumberFunctions.Constructor, 0, null, prototype);

            result.DefineProperty("MAX_VALUE", JsBox.MaxValue, PropertyAttributes.None);
            result.DefineProperty("MIN_VALUE", JsBox.MinValue, PropertyAttributes.None);
            result.DefineProperty("NaN", JsBox.NaN, PropertyAttributes.None);
            result.DefineProperty("POSITIVE_INFINITY", JsBox.PositiveInfinity, PropertyAttributes.None);
            result.DefineProperty("NEGATIVE_INFINITY", JsBox.NegativeInfinity, PropertyAttributes.None);

            return result;
        }

        private JsObject BuildRegExpClass()
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            prototype.DefineProperty("toString", RegExpFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toLocaleString", RegExpFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("lastIndex", RegExpFunctions.GetLastIndex, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("exec", RegExpFunctions.Exec, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("test", RegExpFunctions.Test, 0, PropertyAttributes.DontEnum);

            return CreateFunction("RegExp", RegExpFunctions.Constructor, 0, null, prototype);
        }

        private JsObject BuildStringClass()
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            prototype.DefineProperty("split", StringFunctions.Split, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty("replace", StringFunctions.Replace, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toString", StringFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toLocaleString", StringFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("match", StringFunctions.Match, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("localeCompare", StringFunctions.LocaleCompare, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("substring", StringFunctions.Substring, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty("substr", StringFunctions.Substr, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty("search", StringFunctions.Search, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("valueOf", StringFunctions.ValueOf, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("concat", StringFunctions.Concat, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("charAt", StringFunctions.CharAt, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("charCodeAt", StringFunctions.CharCodeAt, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("lastIndexOf", StringFunctions.LastIndexOf, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty("indexOf", StringFunctions.IndexOf, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toLowerCase", StringFunctions.ToLowerCase, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toLocaleLowerCase", StringFunctions.ToLocaleLowerCase, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toUpperCase", StringFunctions.ToUpperCase, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toLocaleUpperCase", StringFunctions.ToLocaleUpperCase, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("slice", StringFunctions.Slice, 2, PropertyAttributes.DontEnum);
            prototype.DefineAccessor("length", StringFunctions.GetLength, null, PropertyAttributes.DontEnum);

            var result = CreateFunction("String", StringFunctions.Constructor, 0, null, prototype);

            result.DefineProperty("fromCharCode", StringFunctions.FromCharCode, 1, PropertyAttributes.DontEnum);

            return result;
        }

        private JsObject BuildMathClass()
        {
            var result = CreateObject(ObjectClass.Prototype);

            result.DefineProperty("abs", MathFunctions.Abs, 1, PropertyAttributes.DontEnum);
            result.DefineProperty("acos", MathFunctions.Acos, 1, PropertyAttributes.DontEnum);
            result.DefineProperty("asin", MathFunctions.Asin, 1, PropertyAttributes.DontEnum);
            result.DefineProperty("atan", MathFunctions.Atan, 1, PropertyAttributes.DontEnum);
            result.DefineProperty("atan2", MathFunctions.Atan2, 2, PropertyAttributes.DontEnum);
            result.DefineProperty("ceil", MathFunctions.Ceil, 1, PropertyAttributes.DontEnum);
            result.DefineProperty("cos", MathFunctions.Cos, 1, PropertyAttributes.DontEnum);
            result.DefineProperty("exp", MathFunctions.Exp, 1, PropertyAttributes.DontEnum);
            result.DefineProperty("floor", MathFunctions.Floor, 1, PropertyAttributes.DontEnum);
            result.DefineProperty("log", MathFunctions.Log, 1, PropertyAttributes.DontEnum);
            result.DefineProperty("max", MathFunctions.Max, 1, PropertyAttributes.DontEnum);
            result.DefineProperty("min", MathFunctions.Min, 1, PropertyAttributes.DontEnum);
            result.DefineProperty("pow", MathFunctions.Pow, 2, PropertyAttributes.DontEnum);
            result.DefineProperty("random", MathFunctions.Random, 0, PropertyAttributes.DontEnum);
            result.DefineProperty("round", MathFunctions.Round, 1, PropertyAttributes.DontEnum);
            result.DefineProperty("sin", MathFunctions.Sin, 1, PropertyAttributes.DontEnum);
            result.DefineProperty("sqrt", MathFunctions.Sqrt, 1, PropertyAttributes.DontEnum);
            result.DefineProperty("tan", MathFunctions.Tan, 1, PropertyAttributes.DontEnum);
            result.DefineProperty("E", JsNumber.Box(Math.E), PropertyAttributes.DontEnum);
            result.DefineProperty("LN2", JsNumber.Box(Math.Log(2)), PropertyAttributes.DontEnum);
            result.DefineProperty("LN10", JsNumber.Box(Math.Log(10)), PropertyAttributes.DontEnum);
            result.DefineProperty("LOG2E", JsNumber.Box(Math.Log(Math.E, 2)), PropertyAttributes.DontEnum);
            result.DefineProperty("PI", JsNumber.Box(Math.PI), PropertyAttributes.DontEnum);
            result.DefineProperty("SQRT1_2", JsNumber.Box(Math.Sqrt(0.5)), PropertyAttributes.DontEnum);
            result.DefineProperty("SQRT2", JsNumber.Box(Math.Sqrt(2)), PropertyAttributes.DontEnum);

            return result;
        }

        private JsObject BuildErrorClass(string name)
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            prototype.DefineProperty("name", JsString.Box(name), PropertyAttributes.DontEnum | PropertyAttributes.DontDelete | PropertyAttributes.ReadOnly);
            prototype.DefineProperty("toString", ErrorFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty("toLocaleString", ErrorFunctions.ToString, 0, PropertyAttributes.DontEnum);

            return CreateFunction(name, ErrorFunctions.Constructor, 0, null, prototype);
        }
    }
}
