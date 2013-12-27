using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Jint.Benchmarks.Engines;

namespace Jint.Benchmarks.Suites
{
    public class V8BenchMarkTestSuite : TestSuite
    {
        public V8BenchMarkTestSuite(string basePath, IEngine engine)
            : base(Path.Combine(basePath, "V8"), engine)
        {
        }

        public override string SuiteName
        {
            get { return "V8 Benchmark Suite - version 6"; }
        }

        protected override IEnumerable<string> EnumerateTests()
        {
            return base.EnumerateTests()
                .Where(p => Path.GetFileName(p) != "base.js")
                .Where(p => Path.GetFileName(p) != "run.js")
                .Where(p => Path.GetFileName(p) != "earley-boyer.js") // This one doesn't yet run in Jint
            ;
        }

        protected override IContext CreateContext()
        {
            var ctx = base.CreateContext();

            ctx.ExecuteFile(Path.Combine(BasePath, "base.js"));

            return ctx;
        }

        protected override ITestResult ExecuteTest(IContext ctx, string test)
        {
            var errors = new StringBuilder();
            string score = null;

            ctx.SetFunction(
                "NotifyResult",
                new Action<string, string>((name, result) => { })
            );
            ctx.SetFunction(
                "NotifyError",
                new Action<string, object>((name, error) => errors.AppendLine(name + ": " + error.ToString()))
            );
            ctx.SetFunction(
                "NotifyScore",
                new Action<string>(p => score = p)
            );

            try
            {
                ctx.ExecuteFile(test);
                ctx.Execute(@"
                    BenchmarkSuite.RunSuites({
                        NotifyResult: NotifyResult,
                        NotifyError: NotifyError,
                        NotifyScore: NotifyScore
                });");
            }
            catch (Exception ex)
            {
                errors.AppendLine("Exception: " + ex.GetBaseException().Message);
            }

            if (errors.Length > 0)
                return new TestError(errors.ToString());

            return new TestResult(Double.Parse(score, CultureInfo.InvariantCulture));
        }

        protected override ITestResult AggregateResults(IList<ITestResult> results)
        {
            if (results.Any(p => p.IsError))
                return new TestError("Could not aggregate the score, because errors exist.");

            var product = 1.0;

            foreach (var result in results)
            {
                product *= ((TestResult)result).Score;
            }

            var geometricMean = Math.Pow(product, 1.0 / results.Count);

            return new TestResult(Math.Round(geometricMean, 2));
        }

        private class TestResult : ITestResult
        {
            public double Score { get; private set; }

            public bool IsError
            {
                get { return false; }
            }

            public TestResult(double score)
            {
                Score = score;
            }

            public override string ToString()
            {
                return Score.ToString("0.0");
            }

            public TestRelativeResult GetRelativeResult(ITestResult other)
            {
                // Higher is better.

                return new TestRelativeResult(Score / ((TestResult)other).Score);
            }
        }
    }
}
