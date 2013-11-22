using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Mozilla
{
    public class ArrayTests : MozillaFixture
    {
        public ArrayTests()
            : base("Array")
        {
        }

        [TestCase("15.4.4.11-01.js")]
        [TestCase("15.4.4.3-1.js")]
        [TestCase("15.4.4.4-001.js")]
        [TestCase("15.4.5.1-01.js")]
        [TestCase("15.5.4.8-01.js")]
        [TestCase("regress-101488.js")]
        [TestCase("regress-130451.js")]
        [TestCase("regress-322135-01.js")]
        [TestCase("regress-322135-02.js")]
        [TestCase("regress-322135-03.js")]
        [TestCase("regress-322135-04.js")]
        [TestCase("regress-387501.js")]
        [TestCase("regress-421325.js")]
        [TestCase("regress-430717.js")]
        [TestCase("regress-488989.js")]
        public void ShouldRunArrayTests(string script)
        {
            RunFile(script);
        }
    }
}
