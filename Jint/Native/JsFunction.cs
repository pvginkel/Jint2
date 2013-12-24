using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    public delegate object JsFunction(
        JintRuntime runtime,
        object @this,
        JsObject callee,
        object[] arguments
    );
}
