using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Tests.Fixtures
{
    public struct Size
    {
        public int Width;
        public int Height;
    }

    public enum FooEnum
    {
        Name = 1,
        GetType = 2,
        IsEnum = 3,
        System = 4
    }

    public class Box
    {
        // public fields
        public int width;
        public int height;

        // public properties
        public int Width { get; set; }
        public int Height { get; set; }

        public void SetSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Foo(int a, object b)
        {
            return a;
        }

        public int Foo(int a)
        {
            return a;
        }

        public void Write(object value)
        {
            Console.WriteLine(value);
        }
    }
}
