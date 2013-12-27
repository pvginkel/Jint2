using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Jint.Benchmarks.Engines;
using Jint.Benchmarks.Suites;

namespace Jint.Benchmarks
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
            ConsoleEx.WriteLine(ConsoleColor.Red, "The test suite should be run in release mode!!!");
            Console.WriteLine();
#endif

            if (Debugger.IsAttached)
            {
                ConsoleEx.WriteLine(ConsoleColor.Red, "The test suite should not be run with the debugger attached!!!");
                Console.WriteLine();
            }

            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            RunTestSuite(typeof(SunSpiderTestSuite));
            RunTestSuite(typeof(V8BenchMarkTestSuite));
        }

        private static void RunTestSuite(Type testSuiteType)
        {
            string basePath = Path.Combine(
                Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(typeof(Program).Assembly.Location))),
                "Suites"
            );

            var resultSets = new List<Dictionary<string, ITestResult>>();

            var engines = new IEngine[] { new Engines.JintEngine(), new IronJsEngine(), new JurassicEngine() };

            foreach (var engine in engines)
            {
                var testSuite = (TestSuite)Activator.CreateInstance(testSuiteType, new object[] { basePath, engine });

                resultSets.Add(testSuite.Run());
            }

            // Create the relative test results.

            var lines = new List<ResultLine>();

            foreach (string test in resultSets[0].Keys)
            {
                lines.Add(new ResultLine(
                    Path.GetFileName(test),
                    resultSets.Select(p => p[test]).ToList()
                ));
            }

            lines.Sort((a, b) => a.RelativeResults[0].Difference.CompareTo(b.RelativeResults[0].Difference));

            // Calculate the column widths.

            const string testHeader = "Test";

            var columns = new int[engines.Length];

            columns[0] = testHeader.Length;

            for (int i = 1; i < engines.Length; i++)
            {
                columns[i] = engines[i].Name.Length;
            }

            foreach (var line in lines)
            {
                columns[0] = Math.Max(columns[0], line.Test.Length);

                for (int i = 0; i < line.RelativeResults.Count; i++)
                {
                    columns[i + 1] = Math.Max(columns[i + 1], line.RelativeResults[i].ToString().Length);
                }
            }

            // Print the header.

            ConsoleEx.Write(ConsoleColor.DarkCyan, "Test results relative to ");
            ConsoleEx.WriteLine(ConsoleColor.Cyan, engines[0].Name);
            ConsoleEx.WriteLine(ConsoleColor.DarkCyan, "=================================================");
            Console.WriteLine();

            WriteGridLine(
                columns,
                new[] { ConsoleColor.Cyan },
                new[] { testHeader }.Concat(engines.Skip(1).Select(p => p.Name))
            );
            WriteGridSeparator(columns);

            foreach (var line in lines)
            {
                var colors = new List<ConsoleColor>();

                colors.Add(ConsoleColor.Gray);

                for (int i = 0; i < line.RelativeResults.Count; i++)
                {
                    colors.Add(line.RelativeResults[i].Difference < 1 ? ConsoleColor.Green : ConsoleColor.Red);
                }

                WriteGridLine(
                    columns,
                    colors.ToArray(),
                    new[] { line.Test }.Concat(line.RelativeResults.Select(p => p.ToString()))
                );
            }

            Console.WriteLine();
        }

        private static void WriteGridLine(int[] columns, ConsoleColor[] colors, IEnumerable<string> values)
        {
            int offset = 0;
            bool hadOne = false;

            foreach (string value in values)
            {
                if (hadOne)
                    ConsoleEx.Write(ConsoleColor.DarkCyan, " | ");
                else
                    hadOne = true;

                var color = colors[Math.Min(offset, colors.Length - 1)];

                ConsoleEx.Write(color, value);
                Console.Write(new string(' ', columns[offset++] - value.Length));
            }

            Console.WriteLine();
        }

        private static void WriteGridSeparator(int[] columns)
        {
            bool hadOne = false;

            foreach (int column in columns)
            {
                if (hadOne)
                    ConsoleEx.Write(ConsoleColor.DarkCyan, "-+-");
                else
                    hadOne = true;

                ConsoleEx.Write(ConsoleColor.DarkCyan, new string('-', column));
            }

            Console.WriteLine();
        }

        private class ResultLine
        {
            public string Test { get; private set; }
            public List<TestRelativeResult> RelativeResults { get; private set; }

            public ResultLine(string test, List<ITestResult> results)
            {
                Test = test;
                RelativeResults = new List<TestRelativeResult>();

                for (int i = 1; i < results.Count; i++)
                {
                    RelativeResults.Add(results[i].GetRelativeResult(results[0]));
                }
            }
        }
    }
}
