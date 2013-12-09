using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native.Interop
{
    public delegate object WrappedConstructor(JsGlobal global, object[] arguments);

    public delegate object WrappedIndexerGetter(JsGlobal global, JsObject @this, object index);

    public delegate void WrappedIndexerSetter(JsGlobal global, JsObject @this, object index, object value);

}
