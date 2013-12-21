using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal abstract class BoundNode
    {
        public abstract void Accept(BoundTreeVisitor visitor);
        public abstract T Accept<T>(BoundTreeVisitor<T> visitor);
        public abstract BoundNodeType NodeType { get; }

        internal string DebuggerDisplay
        {
            get
            {
                using (var writer = new StringWriter())
                {
                    new BoundTreePrettyPrintVisitor(writer).Visit(this);
                    return writer.GetStringBuilder().ToString();
                }
            }
        }
    }
}
