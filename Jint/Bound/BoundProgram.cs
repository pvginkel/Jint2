using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    internal class BoundProgram
    {
        public BoundBody Body { get; private set; }

        public BoundProgram(BoundBody body)
        {
            if (body == null)
                throw new ArgumentNullException("body");

            Body = body;
        }

        public BoundProgram Update(BoundBody body)
        {
            if (body == Body)
                return this;

            return new BoundProgram(body);
        }
    }
}
