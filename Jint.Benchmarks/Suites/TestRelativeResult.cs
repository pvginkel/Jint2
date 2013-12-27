using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jint.Benchmarks.Suites
{
    public class TestRelativeResult
    {
        public double Difference { get; private set; }

        public TestRelativeResult(double difference)
        {
            Difference = difference;
        }

        public override string ToString()
        {
            return (Difference * 100).ToString("0") + "%";
        }
    }
}
