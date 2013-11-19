using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using Jint.Tests;

namespace Jint.Play
{
    class Program
    {
        static void Main(string[] args)
        {
            const string prefix = "Jint.Tests.SunSpider.";
            var script = prefix + "access-fannkuch.js";

            var assembly = typeof(SunSpider).Assembly;
            var program = new StreamReader(assembly.GetManifestResourceStream(script)).ReadToEnd();

            var jint = new JintEngine();

            for (int i = 0; i < 10; i++)
            {
                var sw = Stopwatch.StartNew();

                jint.Run(program);

                Console.WriteLine(sw.Elapsed);
            }
        }
    }
}

