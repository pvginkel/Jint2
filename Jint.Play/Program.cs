using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Jint.Play
{
    class Program
    {
        static void Main(string[] args)
        {
            const string fileName = @"..\..\..\Jint.Tests\SunSpider\Tests\controlflow-recursive.js";

            var jint = new JintEngine();

#if false
            jint.ExecuteFile(fileName);
            Console.WriteLine("Attach");
            Console.ReadLine();
            jint.ExecuteFile(fileName);
#else

            jint.ExecuteFile(fileName);

            var times = new TimeSpan[20];
            int timeOffset = 0;
            var lowest = new TimeSpan();

            // Perform the iterations.

            for (int i = 0; ; i++)
            {
                long memoryBefore = GC.GetTotalMemory(true);

                var stopwatch = Stopwatch.StartNew();

                jint.ExecuteFile(fileName);

                var elapsed = stopwatch.Elapsed;

                long memoryAfter = GC.GetTotalMemory(false);

                times[timeOffset++] = elapsed;
                if (timeOffset == times.Length)
                    timeOffset = 0;

                if (times[times.Length - 1].Ticks != 0)
                {
                    var average = new TimeSpan(times.Sum(p => p.Ticks) / times.Length);

                    if (lowest.Ticks == 0 || average.Ticks < lowest.Ticks)
                        lowest = average;

                    Console.WriteLine(
                        "This run: {0}, average: {1}, lowest: {2}, memory usage: {3}",
                        elapsed.ToString("s\\.fffff"),
                        average.ToString("s\\.fffff"),
                        lowest.ToString("s\\.fffff"),
                        NiceMemory(memoryAfter - memoryBefore)
                    );
                }
            }
#endif
        }

        private static object NiceMemory(long bytes)
        {
            double value = bytes;
            if (value > 1024)
            {
                value /= 1024;

                if (value > 1024)
                {
                    value /= 1024;

                    if (value > 1024)
                    {
                        value /= 1024;
                        return value.ToString("0.0") + "Gb";
                    }

                    return value.ToString("0.0") + "Mb";
                }

                return value.ToString("0.0") + "Kb";
            }

            return bytes + "b";
        }
    }
}

