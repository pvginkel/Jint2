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
            var functionPrototype = CreateNakedFunction(null, FunctionFunctions.BaseConstructor, 0, objectPrototype);

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
            prototype.DefineProperty(Id.toString, ObjectFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toLocaleString, ObjectFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.valueOf, ObjectFunctions.ValueOf, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.hasOwnProperty, ObjectFunctions.HasOwnProperty, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.isPrototypeOf, ObjectFunctions.IsPrototypeOf, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.propertyIsEnumerable, ObjectFunctions.PropertyIsEnumerable, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getPrototypeOf, ObjectFunctions.GetPrototypeOf, 1, PropertyAttributes.DontEnum);

            if (HasOption(Options.EcmaScript5))
            {
                prototype.DefineProperty(Id.defineProperty, ObjectFunctions.DefineProperty, 1, PropertyAttributes.DontEnum);
                prototype.DefineProperty(Id.__lookupGetter__, ObjectFunctions.LookupGetter, 1, PropertyAttributes.DontEnum);
                prototype.DefineProperty(Id.__lookupSetter__, ObjectFunctions.LookupSetter, 1, PropertyAttributes.DontEnum);
            }

            return CreateNakedFunction("Object", ObjectFunctions.Constructor, 0, prototype);
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
            result.IsClr = false;

            return result;
        }

        private void InitializeFunctionClass()
        {
            var prototype = FunctionClass.Prototype;

            prototype.DefineProperty(Id.constructor, FunctionClass, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.call, FunctionFunctions.Call, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.apply, FunctionFunctions.Apply, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toString, FunctionFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toLocaleString, FunctionFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineAccessor(Id.length, FunctionFunctions.GetLength, FunctionFunctions.SetLength, PropertyAttributes.DontEnum);
        }

        private JsObject BuildArrayClass()
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            prototype.DefineAccessor(Id.length, ArrayFunctions.GetLength, ArrayFunctions.SetLength, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toString, ArrayFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toLocaleString, ArrayFunctions.ToLocaleString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.concat, ArrayFunctions.Concat, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.join, ArrayFunctions.Join, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.pop, ArrayFunctions.Pop, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.push, ArrayFunctions.Push, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.reverse, ArrayFunctions.Reverse, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.shift, ArrayFunctions.Shift, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.slice, ArrayFunctions.Slice, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.sort, ArrayFunctions.Sort, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.splice, ArrayFunctions.Splice, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.unshift, ArrayFunctions.UnShift, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.indexOf, ArrayFunctions.IndexOf, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.lastIndexOf, ArrayFunctions.LastIndexOf, 1, PropertyAttributes.DontEnum);

            return CreateNakedFunction("Array", ArrayFunctions.Constructor, 0, prototype);
        }

        private JsObject BuildBooleanClass()
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            prototype.DefineProperty(Id.toString, BooleanFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toLocaleString, BooleanFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.valueOf, BooleanFunctions.ValueOf, 0, PropertyAttributes.DontEnum);

            return CreateNakedFunction("Boolean", BooleanFunctions.Constructor, 0, prototype);
        }

        private JsObject BuildDateClass()
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            prototype.DefineProperty(Id.toString, DateFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toDateString, DateFunctions.ToDateString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toTimeString, DateFunctions.ToTimeString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toLocaleString, DateFunctions.ToLocaleString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toLocaleDateString, DateFunctions.ToLocaleDateString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toLocaleTimeString, DateFunctions.ToLocaleTimeString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.valueOf, DateFunctions.ValueOf, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getTime, DateFunctions.GetTime, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getFullYear, DateFunctions.GetFullYear, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getUTCFullYear, DateFunctions.GetUTCFullYear, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getMonth, DateFunctions.GetMonth, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getUTCMonth, DateFunctions.GetUTCMonth, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getDate, DateFunctions.GetDate, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getUTCDate, DateFunctions.GetUTCDate, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getDay, DateFunctions.GetDay, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getUTCDay, DateFunctions.GetUTCDay, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getHours, DateFunctions.GetHours, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getUTCHours, DateFunctions.GetUTCHours, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getMinutes, DateFunctions.GetMinutes, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getUTCMinutes, DateFunctions.GetUTCMinutes, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getSeconds, DateFunctions.GetSeconds, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getUTCSeconds, DateFunctions.GetUTCSeconds, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getMilliseconds, DateFunctions.GetMilliseconds, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getUTCMilliseconds, DateFunctions.GetUTCMilliseconds, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.getTimezoneOffset, DateFunctions.GetTimezoneOffset, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.setTime, DateFunctions.SetTime, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.setMilliseconds, DateFunctions.SetMilliseconds, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.setUTCMilliseconds, DateFunctions.SetUTCMilliseconds, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.setSeconds, DateFunctions.SetSeconds, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.setUTCSeconds, DateFunctions.SetUTCSeconds, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.setMinutes, DateFunctions.SetMinutes, 3, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.setUTCMinutes, DateFunctions.SetUTCMinutes, 3, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.setHours, DateFunctions.SetHours, 4, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.setUTCHours, DateFunctions.SetUTCHours, 4, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.setDate, DateFunctions.SetDate, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.setUTCDate, DateFunctions.SetUTCDate, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.setMonth, DateFunctions.SetMonth, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.setUTCMonth, DateFunctions.SetUTCMonth, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.setFullYear, DateFunctions.SetFullYear, 3, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.setUTCFullYear, DateFunctions.SetUTCFullYear, 3, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toUTCString, DateFunctions.ToUTCString, 0, PropertyAttributes.DontEnum);

            var result = CreateNakedFunction("Date", DateFunctions.Constructor, 0, prototype);

            result.DefineAccessor(Id.now, DateFunctions.Now, null, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.parse, DateFunctions.Parse, 1, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.parseLocale, DateFunctions.ParseLocale, 1, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.UTC, DateFunctions.UTC, 1, PropertyAttributes.DontEnum);

            return result;
        }

        private JsObject BuildNumberClass()
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            prototype.DefineProperty(Id.toString, NumberFunctions.ToString, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toLocaleString, NumberFunctions.ToLocaleString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toFixed, NumberFunctions.ToFixed, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toExponential, NumberFunctions.ToExponential, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toPrecision, NumberFunctions.ToPrecision, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.valueOf, NumberFunctions.ValueOf, 0, PropertyAttributes.DontEnum);

            var result = CreateNakedFunction("Number", NumberFunctions.Constructor, 0, prototype);

            result.DefineProperty(Id.MAX_VALUE, DoubleBoxes.MaxValue, PropertyAttributes.None);
            result.DefineProperty(Id.MIN_VALUE, DoubleBoxes.MinValue, PropertyAttributes.None);
            result.DefineProperty(Id.NaN, DoubleBoxes.NaN, PropertyAttributes.None);
            result.DefineProperty(Id.POSITIVE_INFINITY, DoubleBoxes.PositiveInfinity, PropertyAttributes.None);
            result.DefineProperty(Id.NEGATIVE_INFINITY, DoubleBoxes.NegativeInfinity, PropertyAttributes.None);

            return result;
        }

        private JsObject BuildRegExpClass()
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            prototype.DefineProperty(Id.toString, RegExpFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toLocaleString, RegExpFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.lastIndex, RegExpFunctions.GetLastIndex, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.exec, RegExpFunctions.Exec, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.test, RegExpFunctions.Test, 0, PropertyAttributes.DontEnum);

            return CreateNakedFunction("RegExp", RegExpFunctions.Constructor, 0, prototype);
        }

        private JsObject BuildStringClass()
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            prototype.DefineProperty(Id.split, StringFunctions.Split, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.replace, StringFunctions.Replace, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toString, StringFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toLocaleString, StringFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.match, StringFunctions.Match, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.localeCompare, StringFunctions.LocaleCompare, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.substring, StringFunctions.Substring, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.substr, StringFunctions.Substr, 2, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.search, StringFunctions.Search, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.valueOf, StringFunctions.ValueOf, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.concat, StringFunctions.Concat, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.charAt, StringFunctions.CharAt, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.charCodeAt, StringFunctions.CharCodeAt, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.lastIndexOf, StringFunctions.LastIndexOf, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.indexOf, StringFunctions.IndexOf, 1, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toLowerCase, StringFunctions.ToLowerCase, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toLocaleLowerCase, StringFunctions.ToLocaleLowerCase, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toUpperCase, StringFunctions.ToUpperCase, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toLocaleUpperCase, StringFunctions.ToLocaleUpperCase, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.slice, StringFunctions.Slice, 2, PropertyAttributes.DontEnum);
            prototype.DefineAccessor(Id.length, StringFunctions.GetLength, null, PropertyAttributes.DontEnum);

            var result = CreateNakedFunction("String", StringFunctions.Constructor, 0, prototype);

            result.DefineProperty(Id.fromCharCode, StringFunctions.FromCharCode, 1, PropertyAttributes.DontEnum);

            return result;
        }

        private JsObject BuildMathClass()
        {
            var result = CreateObject(ObjectClass.Prototype);

            result.DefineProperty(Id.abs, MathFunctions.Abs, 1, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.acos, MathFunctions.Acos, 1, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.asin, MathFunctions.Asin, 1, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.atan, MathFunctions.Atan, 1, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.atan2, MathFunctions.Atan2, 2, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.ceil, MathFunctions.Ceil, 1, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.cos, MathFunctions.Cos, 1, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.exp, MathFunctions.Exp, 1, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.floor, MathFunctions.Floor, 1, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.log, MathFunctions.Log, 1, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.max, MathFunctions.Max, 1, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.min, MathFunctions.Min, 1, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.pow, MathFunctions.Pow, 2, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.random, MathFunctions.Random, 0, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.round, MathFunctions.Round, 1, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.sin, MathFunctions.Sin, 1, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.sqrt, MathFunctions.Sqrt, 1, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.tan, MathFunctions.Tan, 1, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.E, Math.E, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.LN2, Math.Log(2), PropertyAttributes.DontEnum);
            result.DefineProperty(Id.LN10, Math.Log(10), PropertyAttributes.DontEnum);
            result.DefineProperty(Id.LOG2E, Math.Log(Math.E, 2), PropertyAttributes.DontEnum);
            result.DefineProperty(Id.PI, Math.PI, PropertyAttributes.DontEnum);
            result.DefineProperty(Id.SQRT1_2, Math.Sqrt(0.5), PropertyAttributes.DontEnum);
            result.DefineProperty(Id.SQRT2, Math.Sqrt(2), PropertyAttributes.DontEnum);

            return result;
        }

        private JsObject BuildErrorClass(string name)
        {
            var prototype = CreateObject(FunctionClass.Prototype);

            prototype.DefineProperty(Id.name, name, PropertyAttributes.DontEnum | PropertyAttributes.DontDelete | PropertyAttributes.ReadOnly);
            prototype.DefineProperty(Id.toString, ErrorFunctions.ToString, 0, PropertyAttributes.DontEnum);
            prototype.DefineProperty(Id.toLocaleString, ErrorFunctions.ToString, 0, PropertyAttributes.DontEnum);

            return CreateNakedFunction(name, ErrorFunctions.Constructor, 0, prototype);
        }
    }
}
