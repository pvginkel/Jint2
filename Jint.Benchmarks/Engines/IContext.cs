using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Benchmarks.Engines
{
    public interface IContext
    {
        void ExecuteFile(string test);

        void SetFunction(string name, Delegate @delegate);

        void Execute(string script);
    }
}
