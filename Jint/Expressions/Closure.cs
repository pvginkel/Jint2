using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    public class Closure
    {
#if DEBUG
        internal const string ParentFieldName = "__parent";
#else
        internal const string ParentFieldName = "<>parent";
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
