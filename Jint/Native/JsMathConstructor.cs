using System;
using Jint.Delegates;

namespace Jint.Native
{
    [Serializable]
    public class JsMathConstructor : JsObject
    {
        public JsMathConstructor(JsGlobal global)
            : base(global, global.ObjectClass.Prototype)
        {
            var random = new Random();

            #region Functions
            this["abs"] = global.FunctionClass.New((Delegate)new Func<double, JsNumber>(d => JsNumber.Create(Math.Abs(d))));
            this["acos"] = global.FunctionClass.New((Delegate)new Func<double, JsNumber>(d => JsNumber.Create(Math.Acos(d))));
            this["asin"] = global.FunctionClass.New((Delegate)new Func<double, JsNumber>(d => JsNumber.Create(Math.Asin(d))));
            this["atan"] = global.FunctionClass.New((Delegate)new Func<double, JsNumber>(d => JsNumber.Create(Math.Atan(d))));
            this["atan2"] = global.FunctionClass.New((Delegate)new Func<double, double, JsNumber>((y, x) => JsNumber.Create(Math.Atan2(y, x))));
            this["ceil"] = global.FunctionClass.New((Delegate)new Func<double, JsNumber>(d => JsNumber.Create(Math.Ceiling(d))));
            this["cos"] = global.FunctionClass.New((Delegate)new Func<double, JsNumber>(d => JsNumber.Create(Math.Cos(d))));
            this["exp"] = global.FunctionClass.New((Delegate)new Func<double, JsNumber>(d => JsNumber.Create(Math.Exp(d))));
            this["floor"] = global.FunctionClass.New((Delegate)new Func<double, JsNumber>(d => JsNumber.Create(Math.Floor(d))));
            this["log"] = global.FunctionClass.New((Delegate)new Func<double, JsNumber>(d => JsNumber.Create(Math.Log(d))));
            this["max"] = global.FunctionClass.New<JsObject>(MaxImpl);
            this["min"] = global.FunctionClass.New<JsObject>(MinImpl);
            this["pow"] = global.FunctionClass.New((Delegate)new Func<double, double, JsNumber>((a, b) => JsNumber.Create(Math.Pow(a, b))));
            this["random"] = global.FunctionClass.New((Delegate)new Func<double>(random.NextDouble));
            this["round"] = global.FunctionClass.New((Delegate)new Func<double, JsNumber>(d => JsNumber.Create(Math.Round(d))));
            this["sin"] = global.FunctionClass.New((Delegate)new Func<double, JsNumber>(d => JsNumber.Create(Math.Sin(d))));
            this["sqrt"] = global.FunctionClass.New((Delegate)new Func<double, JsNumber>(d => JsNumber.Create(Math.Sqrt(d))));
            this["tan"] = global.FunctionClass.New((Delegate)new Func<double, JsNumber>(d => JsNumber.Create(Math.Tan(d))));
            #endregion

            this["E"] = JsNumber.Create(Math.E);
            this["LN2"] = JsNumber.Create(Math.Log(2));
            this["LN10"] = JsNumber.Create(Math.Log(10));
            this["LOG2E"] = JsNumber.Create(Math.Log(Math.E, 2));
            this["PI"] = JsNumber.Create(Math.PI);
            this["SQRT1_2"] = JsNumber.Create(Math.Sqrt(0.5));
            this["SQRT2"] = JsNumber.Create(Math.Sqrt(2));
        }

        public const string MathType = "object";

        public override string Class
        {
            get { return MathType; }
        }

        public JsInstance MaxImpl(JsObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
            {
                return JsNumber.NegativeInfinity;
            }

            var result = parameters[0].ToNumber();

            foreach (var p in parameters)
            {
                result = Math.Max(p.ToNumber(), result);
            }

            return JsNumber.Create(result);
        }


        public JsInstance MinImpl(JsObject target, JsInstance[] parameters)
        {
            if (parameters.Length == 0)
            {
                return JsNumber.PositiveInfinity;
            }

            var result = parameters[0].ToNumber();

            foreach (var p in parameters)
            {
                result = Math.Min(p.ToNumber(), result);
            }

            return JsNumber.Create(result);
        }
    }
}
