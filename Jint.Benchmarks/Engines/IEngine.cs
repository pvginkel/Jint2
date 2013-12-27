using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Benchmarks.Engines
{
    public interface IEngine
    {
        IContext CreateContext();

        string Name { get; }
    }
}
