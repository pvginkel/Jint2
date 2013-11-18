using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    internal class Closure
    {
#if DEBUG
        internal const string ParentFieldName = "__parent";
        internal const string ArgumentsFieldName = "__arguments";
#else
        internal const string ParentFieldName = "<>parent";
        internal const string ArgumentsFieldName = "<>arguments";
#endif

        public Type Type { get; private set; }
        public Closure Parent { get; private set; }

        public Closure(Type type, Closure parent)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            Type = type;
            Parent = parent;
        }
    }
}
