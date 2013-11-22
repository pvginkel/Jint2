using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Mozilla
{
    public class ExpressionsTests : MozillaFixture
    {
        public ExpressionsTests()
            : base("Expressions")
        {
        }

        [TestCase("11.10-01.js")]
        [TestCase("11.10-02.js")]
        [TestCase("11.10-03.js")]
        [TestCase("11.6.1-1.js")]
        [TestCase("11.7.1-01.js")]
        [TestCase("11.7.2-01.js")]
        [TestCase("11.7.3-01.js")]
        [TestCase("11.9.6-1.js")]
        public void ShouldRunExpressionsTests(string script)
        {
            RunFile(script);
        }
    }
}
