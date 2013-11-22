using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Mozilla
{
    public class NumberTests : MozillaFixture
    {
        public NumberTests()
            : base("Number")
        {
        }

        [TestCase("15.7.4.2-01.js")]
        [TestCase("15.7.4.3-01.js")]
        [TestCase("15.7.4.3-02.js")]
        [TestCase("15.7.4.5-1.js")]
        [TestCase("15.7.4.5-2.js")]
        [TestCase("15.7.4.6-1.js")]
        [TestCase("15.7.4.7-1.js")]
        [TestCase("15.7.4.7-2.js")]
        [TestCase("regress-442242-01.js")]
        public void ShouldRunNumberTests(string script)
        {
            RunFile(script);
        }
    }
}
