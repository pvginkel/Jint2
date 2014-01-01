using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal abstract class JsonProperty
    {
        public string Name { get; private set; }

        protected JsonProperty(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Name = name;
        }
    }
}
