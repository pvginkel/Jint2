using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jint.Benchmarks
{
    internal static class ConsoleEx
    {
        public static void Write(ConsoleColor color, object value)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ForegroundColor = previousColor;
        }

        public static void WriteLine(ConsoleColor color, object value)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ForegroundColor = previousColor;
        }
    }
}
