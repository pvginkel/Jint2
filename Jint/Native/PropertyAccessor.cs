using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    public sealed class PropertyAccessor
    {
        public JsObject Getter { get; private set; }
        public JsObject Setter { get; private set; }

        public PropertyAccessor(JsObject getter, JsObject setter)
        {
            if (getter == null)
                throw new ArgumentNullException("getter");

            Getter = getter;
            Setter = setter;
        }

        public object GetValue(object @this)
        {
            return Getter.Global.ExecuteFunction(
                Getter,
                @this,
                JsValue.EmptyArray
            );
        }

        public void SetValue(object @this, object value)
        {
            Setter.Global.ExecuteFunction(
                Setter,
                @this,
                new[] { value }
            );
        }
    }
}
