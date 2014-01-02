using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Ast
{
    internal class Closure
    {
        public ReadOnlyArray<string> Fields { get; private set; }

        public Closure(ReadOnlyArray<string> fields)
        {
            if (fields == null)
                throw new ArgumentNullException("fields");

            Fields = fields;
        }
    }
}
