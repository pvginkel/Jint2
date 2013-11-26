using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class JsException : Exception
    {
        public JsErrorType Type { get; private set; }
        public JsInstance Value { get; private set; }

        public JsException(JsErrorType type)
            : this(type, null)
        {
        }

        public JsException(JsErrorType type, string message)
            : base(message)
        {
            Type = type;
        }

        public JsException(JsInstance value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            Value = value;
        }
    }
}
