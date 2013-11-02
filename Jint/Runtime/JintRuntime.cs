using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Native;

namespace Jint.Runtime
{
    public class JintRuntime
    {
        internal const string GlobalName = "Global";
        internal const string GlobalScopeName = "GlobalScope";

        public JsScope GlobalScope { get; private set; }
        public JsGlobal Global { get; private set; }

        public JintRuntime(IJintBackend backend, Options options)
        {
            if (backend == null)
                throw new ArgumentNullException("backend");

            var global = new JsGlobal(backend, options);

            Global = global;
            GlobalScope = new JsScope(global);

            global["ToBoolean"] = Global.FunctionClass.New(new Func<object, Boolean>(Convert.ToBoolean));
            global["ToByte"] = Global.FunctionClass.New(new Func<object, Byte>(Convert.ToByte));
            global["ToChar"] = Global.FunctionClass.New(new Func<object, Char>(Convert.ToChar));
            global["ToDateTime"] = Global.FunctionClass.New(new Func<object, DateTime>(Convert.ToDateTime));
            global["ToDecimal"] = Global.FunctionClass.New(new Func<object, Decimal>(Convert.ToDecimal));
            global["ToDouble"] = Global.FunctionClass.New(new Func<object, Double>(Convert.ToDouble));
            global["ToInt16"] = Global.FunctionClass.New(new Func<object, Int16>(Convert.ToInt16));
            global["ToInt32"] = Global.FunctionClass.New(new Func<object, Int32>(Convert.ToInt32));
            global["ToInt64"] = Global.FunctionClass.New(new Func<object, Int64>(Convert.ToInt64));
            global["ToSByte"] = Global.FunctionClass.New(new Func<object, SByte>(Convert.ToSByte));
            global["ToSingle"] = Global.FunctionClass.New(new Func<object, Single>(Convert.ToSingle));
            global["ToString"] = Global.FunctionClass.New(new Func<object, String>(Convert.ToString));
            global["ToUInt16"] = Global.FunctionClass.New(new Func<object, UInt16>(Convert.ToUInt16));
            global["ToUInt32"] = Global.FunctionClass.New(new Func<object, UInt32>(Convert.ToUInt32));
            global["ToUInt64"] = Global.FunctionClass.New(new Func<object, UInt64>(Convert.ToUInt64));
        }
    }
}
