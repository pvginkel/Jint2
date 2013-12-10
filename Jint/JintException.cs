using System;
using System.Collections.Generic;
using System.Text;

namespace Jint
{
    public class JintException : Exception
    {
        public JintException()
        {
        }

        public JintException(string message)
            : base(message)
        {
        }

        public JintException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
