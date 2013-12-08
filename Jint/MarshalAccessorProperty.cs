using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Native;

namespace Jint
{
    public class MarshalAccessorProperty
    {
        public int Index { get; private set; }
        public JsObject Getter { get; private set; }
        public JsObject Setter { get; private set; }
        public PropertyAttributes Attributes { get; private set; }

        public MarshalAccessorProperty(int index, JsObject getter, JsObject setter, PropertyAttributes attributes)
        {
            Index = index;
            Getter = getter;
            Setter = setter;
            Attributes = attributes;
        }

        public void DefineProperty(JsObject @object)
        {
            @object.DefineAccessor(
                Index,
                Getter,
                Setter,
                Attributes
            );
        }
    }
}
