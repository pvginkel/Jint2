using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Runtime;

namespace Jint.Native
{
    public delegate JsInstance JsFunction(
        JintRuntime runtime,
        JsInstance @this,
        JsObject callee,
        object closure,
        JsInstance[] arguments,
        JsInstance[] genericArguments
    );
}
