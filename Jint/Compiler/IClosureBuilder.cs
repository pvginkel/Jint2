using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Jint.Bound;
using Jint.Support;

namespace Jint.Compiler
{
    internal interface IClosureBuilder : ITypeBuilder
    {
        BoundClosure Closure { get; }
        ConstructorBuilder Constructor { get; }
        IKeyedCollection<IClosureBuilder, ClosureParentField> ParentFields { get; }

        IClosureFieldBuilder CreateClosureFieldBuilder(BoundClosureField field);
    }
}
