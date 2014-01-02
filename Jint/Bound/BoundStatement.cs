using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Ast;

namespace Jint.Bound
{
    internal abstract class BoundStatement : BoundNode
    {
        public SourceLocation Location { get; private set; }

        protected BoundStatement(SourceLocation location)
        {
            if (location == null)
                throw new ArgumentNullException("location");

            Location = location;
        }
    }
}
