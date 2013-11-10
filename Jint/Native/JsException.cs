using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    [Serializable]
    public class JsException : Exception
    {
        public JsInstance Value { get; set; }

        public JsException()
        {
        }

        public JsException(JsInstance value)
            : base(value.ToSource())
        {
            Value = value;
        }
    }
}
