using System;
using System.Collections.Generic;
using System.Text;
using Jint.Runtime;

namespace Jint.Native.Interop
{
    public delegate object WrappedConstructor(JsGlobal global, JsInstance[] arguments);

    public delegate JsInstance WrappedGetter(JsGlobal global, JsObject @this);

    public delegate void WrappedSetter(JsGlobal global, JsObject @this, JsInstance value);

    public delegate JsInstance WrappedIndexerGetter(JsGlobal global, JsInstance @this, JsInstance index);

    public delegate void WrappedIndexerSetter(JsGlobal global, JsInstance @this, JsInstance index, JsInstance value);

}
