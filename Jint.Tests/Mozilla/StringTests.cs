using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Mozilla
{
    public class StringTests : MozillaFixture
    {
        public StringTests()
            : base("String")
        {
        }

        [TestCase("15.5.4.11.js")]
        [TestCase("15.5.4.14.js")]
        [TestCase("regress-104375.js")]
        [TestCase("regress-189898.js")]
        [TestCase("regress-304376.js")]
        [TestCase("regress-313567.js")]
        [TestCase("regress-392378.js")]
        [TestCase("regress-83293.js")]
        public void ShouldRunStringTests(string script)
        {
            RunFile(script);
        }
    }
}
