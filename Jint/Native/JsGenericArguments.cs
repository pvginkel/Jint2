using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    public class JsGenericArguments
    {
        public object[] Arguments { get; private set; }

        public JsGenericArguments(object[] arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException("arguments");

            Arguments = arguments;
        }
    }
}
