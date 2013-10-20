using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Native;

namespace Jint.Backend.Compiled
{
    public delegate JsInstance CompiledFunctionDelegate(JsDictionaryObject that, JsInstance[] arguments);
}
