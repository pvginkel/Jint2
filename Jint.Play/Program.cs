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
            const string fileName = @"..\..\..\Jint.Tests\SunSpider\Tests\access-fannkuch.js";
            var program = File.ReadAllText(fileName);

            var jint = new JintEngine();

#if false
            jint.Run(program, fileName);
            Console.WriteLine("Attach");
            Console.ReadLine();
            jint.Run(program, fileName);
#else

            jint.Run(program, fileName);

            var times = new TimeSpan[20];
            int timeOffset = 0;
            var lowest = new TimeSpan();

            // Perform the iterations.

            for (int i = 0; ; i++)
            {
                GC.Collect();

                var stopwatch = Stopwatch.StartNew();

                jint.Run(program, fileName);

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

