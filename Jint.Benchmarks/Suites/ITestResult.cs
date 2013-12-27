using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jint.Benchmarks.Suites
{
    public interface ITestResult
    {
        bool IsError { get; }

        TestRelativeResult GetRelativeResult(ITestResult other);
    }
}
