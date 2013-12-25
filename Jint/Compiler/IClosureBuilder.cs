using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Jint.Bound;

namespace Jint.Compiler
{
    internal interface IClosureBuilder : ITypeBuilder
    {
        BoundClosure Closure { get; }
        ConstructorBuilder Constructor { get; }

        IClosureFieldBuilder CreateClosureFieldBuilder(BoundClosureField field);
    }
}
