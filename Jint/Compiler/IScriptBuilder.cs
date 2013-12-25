using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Bound;

namespace Jint.Compiler
{
    internal interface IScriptBuilder : ITypeBuilder
    {
        string FileName { get; }
        IList<IClosureBuilder> Closures { get; }

        void Commit();
        void CommitClosureFields();
        IClosureBuilder CreateClosureBuilder(BoundClosure closure);
    }
}
