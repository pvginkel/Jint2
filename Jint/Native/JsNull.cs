using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    public sealed class JsNull
    {
        public static JsNull Instance = new JsNull();

        private JsNull()
        {
        }

        public override string ToString()
        {
            return "null";
        }
    }
}
