using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Jint.Benchmarks.Engines;

namespace Jint.Benchmarks.Suites
{
    public class SunSpiderTestSuite : TestSuite
    {
        private static readonly double[] Distribution = { Double.NaN, Double.NaN, 12.71, 4.30, 3.18, 2.78, 2.57, 2.45, 2.36, 2.31, 2.26, 2.23, 2.20, 2.18, 2.16, 2.14, 2.13, 2.12, 2.11, 2.10, 2.09, 2.09, 2.08, 2.07, 2.07, 2.06, 2.06, 2.06, 2.05, 2.05, 2.05, 2.04, 2.04, 2.04, 2.03, 2.03, 2.03, 2.03, 2.03, 2.02, 2.02, 2.02, 2.02, 2.02, 2.02, 2.02, 2.01, 2.01, 2.01, 2.01, 2.01, 2.01, 2.01, 2.01, 2.01, 2.00, 2.00, 2.00, 2.00, 2.00, 2.00, 2.00, 2.00, 2.00, 2.00, 2.00, 2.00, 2.00, 2.00, 2.00, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.99, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.98, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.97, 1.96 };
        private const int Runs = 20;

        public override string SuiteName
        {
            get { return "SunSpider 0.9.1"; }
        }

        public SunSpiderTestSuite(string basePath, IEngine engine)
            : base(Path.Combine(basePath, "SunSpider"), engine)
        {
        }

        protected override ITestResult ExecuteTest(IContext ctx, string test)
        {
            var times = new List<long>();

            try
            {
                // Warmup.
                ctx.ExecuteFile(test);

                long lowest = long.MaxValue;

                for (int i = 0; i < Runs; i++)
                {
                    // Collect all garbage between runs.
                    GC.Collect(2, GCCollectionMode.Forced);

                    // Run and time.
                    var sw = Stopwatch.StartNew();
                    ctx.ExecuteFile(test);
                    sw.Stop();

                    long elapsed = sw.ElapsedMilliseconds;
                    if (elapsed < lowest)
                    {
                        // If the current elapsed time is less than the lowest
                        // we've had up until now, we restart the run. We do this
                        // to try to limit outside influence.

                        i = -1;
                        lowest = elapsed;
                        times.Clear();
                    }
                    else
                    {
                        times.Add(sw.ElapsedMilliseconds);
                    }
                }
            }
            catch (Exception ex)
            {
                return new TestError(ex.GetBaseException().Message);
            }

            return new TestResult(GetScore(times), times.ToArray());
        }

        protected override ITestResult AggregateResults(IList<ITestResult> results)
        {
            if (results.Any(p => p.IsError))
                return new TestError("Could not aggregate the score, because errors exist.");

            var runs = new long[Runs];

            foreach (var result in results)
            {
                var times = ((TestResult)result).Times;

                for (int i = 0; i < Runs; i++)
                {
                    runs[i] += times[i];
                }
            }

            return new TestResult(GetScore(runs), null);
        }

        private static double GetDistribution(int index)
        {
            return Distribution[Math.Min(index, Distribution.Length - 1)];
        }

        private static string GetScore(IList<long> times)
        {
            var runs = times.Count;

            var mean = times.Sum(p => (double)p) / runs;

            var stdDev = Math.Sqrt(
                (
                    from i in times
                    let delta = i - mean
                    let deltaSq = delta * delta
                    select deltaSq
                ).Sum() / (runs - 1)
            );

            var stdErr = stdDev / Math.Sqrt(Runs);

            var confidence = ((GetDistribution(Runs) * stdErr / mean) * 100);

            return Math.Round(mean, 1).ToString("0.0") + "ms ± " + Math.Round(confidence, 1).ToString("0.0") + "%";
        }

        private class TestResult : ITestResult
        {
            private readonly string _score;

            public long[] Times { get; private set; }

            public bool IsError
            {
                get { return false; }
            }

            public TestResult(string score, long[] times)
            {
                _score = score;
                Times = times;
            }

            public override string ToString()
            {
                return _score;
            }

            public TestRelativeResult GetRelativeResult(ITestResult other)
            {
                double thisMean = Times.Sum(p => (double)p) / Times.Length;
                double otherMean = ((TestResult)other).Times.Sum(p => (double)p) / ((TestResult)other).Times.Length;

                // Lower is better.

                return new TestRelativeResult(otherMean / thisMean);
            }
        }
    }
}
