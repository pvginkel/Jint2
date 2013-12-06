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
            internal static JsBox Abs(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Math.Abs(arguments[0].ToNumber()));
            }

            public static JsBox Acos(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Math.Acos(arguments[0].ToNumber()));
            }

            public static JsBox Asin(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Math.Asin(arguments[0].ToNumber()));
            }

            public static JsBox Atan(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Math.Atan(arguments[0].ToNumber()));
            }

            public static JsBox Atan2(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Math.Atan2(arguments[0].ToNumber(), arguments[1].ToNumber()));
            }

            public static JsBox Ceil(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Math.Ceiling(arguments[0].ToNumber()));
            }

            public static JsBox Cos(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Math.Cos(arguments[0].ToNumber()));
            }

            public static JsBox Exp(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Math.Exp(arguments[0].ToNumber()));
            }

            public static JsBox Floor(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Math.Floor(arguments[0].ToNumber()));
            }

            public static JsBox Log(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Math.Log(arguments[0].ToNumber()));
            }

            public static JsBox Max(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                {
                    return JsBox.NegativeInfinity;
                }

                var result = arguments[0].ToNumber();

                foreach (var p in arguments)
                {
                    result = Math.Max(p.ToNumber(), result);
                }

                return JsNumber.Box(result);
            }


            public static JsBox Min(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                if (arguments.Length == 0)
                {
                    return JsBox.PositiveInfinity;
                }

                var result = arguments[0].ToNumber();

                foreach (var p in arguments)
                {
                    result = Math.Min(p.ToNumber(), result);
                }

                return JsNumber.Box(result);
            }

            public static JsBox Pow(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Math.Pow(arguments[0].ToNumber(), arguments[1].ToNumber()));
            }

            public static JsBox Random(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(runtime.Global.Random.NextDouble());
            }

            public static JsBox Round(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Math.Round(arguments[0].ToNumber()));
            }

            public static JsBox Sin(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Math.Sin(arguments[0].ToNumber()));
            }

            public static JsBox Sqrt(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Math.Sqrt(arguments[0].ToNumber()));
            }

            public static JsBox Tan(JintRuntime runtime, JsBox @this, JsObject callee, object closure, JsBox[] arguments, JsBox[] genericArguments)
            {
                return JsNumber.Box(Math.Tan(arguments[0].ToNumber()));
            }
        }
    }
}
