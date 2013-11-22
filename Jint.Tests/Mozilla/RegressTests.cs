using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Mozilla
{
    public class RegressTests : MozillaFixture
    {
        public RegressTests()
            : base("Regress")
        {
        }

        [TestCase("regress-385393-04.js")]
        [TestCase("regress-419152.js")]
        [TestCase("regress-420087.js")]
        [TestCase("regress-420610.js")]
        [TestCase("regress-441477-01.js")]
        [TestCase("regress-469937.js")]
        public void ShouldRunRegressTests(string script)
        {
            RunFile(script);
        }
    }
}
