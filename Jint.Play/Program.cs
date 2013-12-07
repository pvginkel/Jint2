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
            while (true)
            {
                var sw = Stopwatch.StartNew();

                jint.Run(program);

                Console.WriteLine(sw.Elapsed);
            }
#endif
        }
    }
}

