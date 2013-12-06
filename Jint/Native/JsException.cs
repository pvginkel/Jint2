using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class JsException : Exception
    {
        public JsErrorType Type { get; private set; }
        public JsBox Value { get; private set; }

        public JsException(JsErrorType type)
            : this(type, null)
        {
        }

        public JsException(JsErrorType type, string message)
            : base(message)
        {
            Type = type;
        }

        public JsException(JsBox value)
        {
            if (!value.IsValid)
                throw new ArgumentNullException("value");

            Value = value;
        }
    }
}
