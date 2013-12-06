using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    public delegate JsBox JsFunction(
        JintRuntime runtime,
        JsBox @this,
        JsObject callee,
        object closure,
        JsBox[] arguments,
        JsBox[] genericArguments
    );
}
