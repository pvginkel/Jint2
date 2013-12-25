using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal class Closure
    {
        public const string ArgumentsFieldName = "<>arguments";

        public Closure Parent { get; private set; }
        public IList<string> Fields { get; private set; }

        public Closure(Closure parent, IEnumerable<string> fields)
        {
            if (fields == null)
                throw new ArgumentNullException("fields");

            Parent = parent;
            Fields = fields.ToReadOnly();
        }
    }
}
