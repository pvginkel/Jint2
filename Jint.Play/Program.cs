using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using Jint.Tests;
using Jint.Tests.Support;

namespace Jint.Play
{
    class Program
    {
        static void Main(string[] args)
        {
            var program = File.ReadAllText(@"..\..\..\Jint.Tests\SunSpider\Tests\bitops-bitwise-and.js");

            var jint = new JintEngine();

#if false
            jint.Run(program);
            Console.WriteLine("Attach");
            Console.ReadLine();
            jint.Run(program);
#else

            jint.Run(program);

            var times = new TimeSpan[20];
            int timeOffset = 0;
            var lowest = new TimeSpan();

            // Perform the iterations.

            for (int i = 0; ; i++)
            {
                GC.Collect();

                var stopwatch = Stopwatch.StartNew();

                jint.Run(program);

                var elapsed = stopwatch.Elapsed;

                times[timeOffset++] = elapsed;
                if (timeOffset == times.Length)
                    timeOffset = 0;

                if (times[times.Length - 1].Ticks != 0)
                {
                    var average = new TimeSpan(times.Sum(p => p.Ticks) / times.Length);

                    if (lowest.Ticks == 0 || average.Ticks < lowest.Ticks)
                        lowest = average;

                    Console.WriteLine(
                        "This run: {0}, average: {1}, lowest: {2}",
                        elapsed.ToString("s\\.fffff"),
                        average.ToString("s\\.fffff"),
                        lowest.ToString("s\\.fffff")
                    );
                }
            }
#endif
        }
    }
}

