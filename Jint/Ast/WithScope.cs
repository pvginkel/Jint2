using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Ast
{
    internal class WithScope
    {
        public WithScope Parent { get; private set; }
        public IIdentifier Identifier { get; private set; }

        public WithScope(WithScope parent, IIdentifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException("identifier");

            Parent = parent;
            Identifier = identifier;
        }
    }
}
