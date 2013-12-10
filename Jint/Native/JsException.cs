using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    public class JsException : Exception
    {
        public JsErrorType Type { get; private set; }
        public object Value { get; private set; }

        public JsException(JsErrorType type)
            : this(type, null)
        {
        }

        public JsException(JsErrorType type, string message)
            : base(message)
        {
            Type = type;
        }

        public JsException(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            Value = value;
        }
    }
}
