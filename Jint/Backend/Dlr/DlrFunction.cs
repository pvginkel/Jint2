using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Native;
using Jint.Runtime;

namespace Jint.Backend.Dlr
{
    internal class DlrFunction : JsFunction
    {
        private readonly DlrFunctionDelegate _function;
        private readonly object _closure;
        private readonly JintRuntime _runtime;

        public DlrFunction(JsGlobal global, DlrFunctionDelegate function, JsObject prototype, object closure, JintRuntime runtime)
            : base(global, prototype)
        {
            if (function == null)
                throw new ArgumentNullException("function");

            _function = function;
            _closure = closure;
            _runtime = runtime;
        }

        public override JsFunctionResult Execute(JsInstance @this, JsInstance[] parameters, Type[] genericArguments)
        {
            var result = _function(_runtime, @this, this, _closure, parameters);

            return new JsFunctionResult(result, @this);
        }

        public override string ToString()
        {
            return String.Format("function {0}() {{ [compiled code] }}", _function.Method.Name);
        }
    }
}
