using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Jint.Bound;

namespace Jint.Compiler
{
    internal interface IClosureFieldBuilder
    {
        BoundClosureField ClosureField { get; }
        FieldBuilder Field { get; }
    }
}
