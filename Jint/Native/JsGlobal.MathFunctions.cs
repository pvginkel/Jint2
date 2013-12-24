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
            internal static object Abs(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return Math.Abs(JsValue.ToNumber(arguments[0]));
            }

            public static object Acos(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return Math.Acos(JsValue.ToNumber(arguments[0]));
            }

            public static object Asin(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return Math.Asin(JsValue.ToNumber(arguments[0]));
            }

            public static object Atan(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return Math.Atan(JsValue.ToNumber(arguments[0]));
            }

            public static object Atan2(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return Math.Atan2(JsValue.ToNumber(arguments[0]), JsValue.ToNumber(arguments[1]));
            }

            public static object Ceil(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return Math.Ceiling(JsValue.ToNumber(arguments[0]));
            }

            public static object Cos(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return Math.Cos(JsValue.ToNumber(arguments[0]));
            }

            public static object Exp(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return Math.Exp(JsValue.ToNumber(arguments[0]));
            }

            public static object Floor(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return Math.Floor(JsValue.ToNumber(arguments[0]));
            }

            public static object Log(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return Math.Log(JsValue.ToNumber(arguments[0]));
            }

            public static object Max(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                if (arguments.Length == 0)
                    return DoubleBoxes.NegativeInfinity;

                var result = JsValue.ToNumber(arguments[0]);

                foreach (var p in arguments)
                {
                    result = Math.Max(JsValue.ToNumber(p), result);
                }

                return result;
            }


            public static object Min(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                if (arguments.Length == 0)
                {
                    return DoubleBoxes.PositiveInfinity;
                }

                var result = JsValue.ToNumber(arguments[0]);

                foreach (var p in arguments)
                {
                    result = Math.Min(JsValue.ToNumber(p), result);
                }

                return result;
            }

            public static object Pow(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return Math.Pow(JsValue.ToNumber(arguments[0]), JsValue.ToNumber(arguments[1]));
            }

            public static object Random(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return runtime.Global.Random.NextDouble();
            }

            public static object Round(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return Math.Round(JsValue.ToNumber(arguments[0]));
            }

            public static object Sin(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return Math.Sin(JsValue.ToNumber(arguments[0]));
            }

            public static object Sqrt(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return Math.Sqrt(JsValue.ToNumber(arguments[0]));
            }

            public static object Tan(JintRuntime runtime, object @this, JsObject callee, object[] arguments)
            {
                return Math.Tan(JsValue.ToNumber(arguments[0]));
            }
        }
    }
}
