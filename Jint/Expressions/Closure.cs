using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal class Closure
    {
        public const string ArgumentsFieldName = "<>arguments";

        public IList<string> Fields { get; private set; }

        public Closure(IEnumerable<string> fields)
        {
            if (fields == null)
                throw new ArgumentNullException("fields");

            Fields = fields.ToReadOnly();
        }
    }
}
