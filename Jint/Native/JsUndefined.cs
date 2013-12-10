using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    public sealed class JsUndefined
    {
        internal string Name { get; private set; }

        public static JsUndefined Instance = new JsUndefined();

        private JsUndefined()
        {
        }

        internal JsUndefined(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Name = name;
        }

        public override string ToString()
        {
            return "undefined";
        }
    }
}
