using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jint.Benchmarks.Suites
{
    public class TestError : ITestResult
    {
        private readonly string _error;

        public bool IsError
        {
            get { return true; }
        }

        public TestError(string error)
        {
            if (error == null)
                throw new ArgumentNullException("error");

            _error = error;
        }

        public override string ToString()
        {
            return _error.TrimEnd();
        }

        public TestRelativeResult GetRelativeResult(ITestResult other)
        {
            throw new InvalidOperationException();
        }
    }
}
