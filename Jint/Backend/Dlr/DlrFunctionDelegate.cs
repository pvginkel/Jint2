using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Native;
using Jint.Runtime;

namespace Jint.Backend.Dlr
{
    public delegate JsInstance DlrFunctionDelegate(JintRuntime runtime, JsDictionaryObject @this, object closure, JsInstance[] arguments);
}
