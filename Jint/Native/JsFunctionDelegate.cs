using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Runtime;

namespace Jint.Native
{
    public delegate JsInstance JsFunctionDelegate(
        JintRuntime runtime,
        JsInstance @this,
        JsFunction callee,
        object closure,
        JsInstance[] arguments,
        JsInstance[] genericArguments
    );
}
