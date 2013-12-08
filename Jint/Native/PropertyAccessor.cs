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

        public JsBox GetValue(JsBox @this)
        {
            return Getter.Global.ExecuteFunction(
                Getter,
                @this,
                JsBox.EmptyArray,
                null
            );
        }

        public void SetValue(JsBox @this, JsBox value)
        {
            Setter.Global.ExecuteFunction(
                Setter,
                @this,
                new[] { value },
                null
            );
        }
    }
}
