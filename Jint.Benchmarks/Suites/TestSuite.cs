using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Jint.Benchmarks.Engines;

namespace Jint.Benchmarks.Suites
{
    public abstract class TestSuite
    {
        public IEngine Engine { get; private set; }
        public string BasePath { get; private set; }

        public abstract string SuiteName { get; }

        protected TestSuite(string basePath, IEngine engine)
        {
            Engine = engine;
            BasePath = basePath;
        }

        protected virtual IEnumerable<string> EnumerateTests()
        {
            return Directory.GetFiles(BasePath, "*.js", SearchOption.AllDirectories);
        }

        protected virtual IContext CreateContext()
        {
            return Engine.CreateContext();
        }

        public Dictionary<string, ITestResult> Run()
        {
            var tests = EnumerateTests().ToList();
            var results = new Dictionary<string, ITestResult>();

            ConsoleEx.Write(ConsoleColor.DarkCyan, SuiteName);
            ConsoleEx.WriteLine(ConsoleColor.Cyan, " on " + Engine.Name);
            ConsoleEx.WriteLine(ConsoleColor.DarkCyan, "=================================================");

            foreach (var test in tests)
            {
                Console.Write(Path.GetFileName(test));

                var ctx = CreateContext();

                var result = ExecuteTest(ctx, test);

                results.Add(test, result);

                Console.Write(": ");

                ConsoleEx.WriteLine(
                    result.IsError ? ConsoleColor.Red : ConsoleColor.Green,
                    result.ToString()
                );
            }

            Console.WriteLine();
            ConsoleEx.Write(ConsoleColor.DarkCyan, "Whole Suite");

            var suiteResult = AggregateResults(results.Values.ToList());

            Console.Write(": ");

            ConsoleEx.WriteLine(
                suiteResult.IsError ? ConsoleColor.Red : ConsoleColor.Green,
                suiteResult.ToString()
            );

            Console.WriteLine();

            return results;
        }

        protected abstract ITestResult ExecuteTest(IContext ctx, string test);

        protected abstract ITestResult AggregateResults(IList<ITestResult> results);
    }
}
