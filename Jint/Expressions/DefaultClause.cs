using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Expressions
{
    public class DefaultClause : ISourceLocation
    {
        public BlockSyntax Body { get; private set; }
        public SourceLocation Location { get; private set; }

        public DefaultClause(BlockSyntax body, SourceLocation location)
        {
            if (body == null)
                throw new ArgumentNullException("body");
            if (location == null)
                throw new ArgumentNullException("location");

            Body = body;
            Location = location;
        }
    }
}
