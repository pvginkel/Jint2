using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Native
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class JsFunctionSourceAttribute : Attribute
    {
        public string Source { get; private set; }

        public JsFunctionSourceAttribute(string source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            Source = source;
        }
    }
}
