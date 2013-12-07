using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native.Interop
{
    public delegate object WrappedConstructor(JsGlobal global, JsBox[] arguments);

    public delegate JsBox WrappedGetter(JsGlobal global, JsObject @this);

    public delegate void WrappedSetter(JsGlobal global, JsObject @this, JsBox value);

    public delegate JsBox WrappedIndexerGetter(JsGlobal global, JsObject @this, JsBox index);

    public delegate void WrappedIndexerSetter(JsGlobal global, JsObject @this, JsBox index, JsBox value);

}
