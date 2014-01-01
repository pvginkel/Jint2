using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundMappedArgument
    {
        public BoundArgument Argument { get; private set; }
        public BoundVariable Mapped { get; private set; }

        public BoundMappedArgument(BoundArgument argument, BoundVariable mapped)
        {
            if (argument == null)
                throw new ArgumentNullException("argument");
            if (mapped == null)
                throw new ArgumentNullException("mapped");

            Argument = argument;
            Mapped = mapped;
        }
    }
}
