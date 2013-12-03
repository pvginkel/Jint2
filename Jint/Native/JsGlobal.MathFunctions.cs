using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    partial class JsGlobal
    {
        private static class MathFunctions
        {
            internal static JsInstance Abs(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Math.Abs(arguments[0].ToNumber()));
            }

            public static JsInstance Acos(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Math.Acos(arguments[0].ToNumber()));
            }

            public static JsInstance Asin(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Math.Asin(arguments[0].ToNumber()));
            }

            public static JsInstance Atan(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Math.Atan(arguments[0].ToNumber()));
            }

            public static JsInstance Atan2(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Math.Atan2(arguments[0].ToNumber(), arguments[1].ToNumber()));
            }

            public static JsInstance Ceil(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Math.Ceiling(arguments[0].ToNumber()));
            }

            public static JsInstance Cos(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Math.Cos(arguments[0].ToNumber()));
            }

            public static JsInstance Exp(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Math.Exp(arguments[0].ToNumber()));
            }

            public static JsInstance Floor(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Math.Floor(arguments[0].ToNumber()));
            }

            public static JsInstance Log(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Math.Log(arguments[0].ToNumber()));
            }

            public static JsInstance Max(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                {
                    return JsNumber.NegativeInfinity;
                }

                var result = arguments[0].ToNumber();

                foreach (var p in arguments)
                {
                    result = Math.Max(p.ToNumber(), result);
                }

                return JsNumber.Create(result);
            }


            public static JsInstance Min(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                if (arguments.Length == 0)
                {
                    return JsNumber.PositiveInfinity;
                }

                var result = arguments[0].ToNumber();

                foreach (var p in arguments)
                {
                    result = Math.Min(p.ToNumber(), result);
                }

                return JsNumber.Create(result);
            }

            public static JsInstance Pow(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Math.Pow(arguments[0].ToNumber(), arguments[1].ToNumber()));
            }

            public static JsInstance Random(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(runtime.Global.Random.NextDouble());
            }

            public static JsInstance Round(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Math.Round(arguments[0].ToNumber()));
            }

            public static JsInstance Sin(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Math.Sin(arguments[0].ToNumber()));
            }

            public static JsInstance Sqrt(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Math.Sqrt(arguments[0].ToNumber()));
            }

            public static JsInstance Tan(JintRuntime runtime, JsInstance @this, JsObject callee, object closure, JsInstance[] arguments, JsInstance[] genericArguments)
            {
                return JsNumber.Create(Math.Tan(arguments[0].ToNumber()));
            }
        }
    }
}
