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
            var program = File.ReadAllText(@"..\..\..\Jint.Tests\SunSpider\Tests\access-fannkuch.js");

            var jint = new JintEngine();

#if false
            jint.Run(program);
            Console.WriteLine("Attach");
            Console.ReadLine();
            jint.Run(program);
#else
            // Run a few times to stabilize the results.

            for (int i = 0; i < 5; i++)
            {
                jint.Run(program);
            }

            // Perform the iterations.

            var total = new TimeSpan();

            for (int i = 0; ; i++)
            {
                var stopwatch = Stopwatch.StartNew();

                jint.Run(program);

                var elapsed = stopwatch.Elapsed;

                total += elapsed;

                Console.WriteLine("This run: {0}, average: {1}", elapsed, new TimeSpan(total.Ticks / (i + 1)));
            }
#endif
        }
    }
}

