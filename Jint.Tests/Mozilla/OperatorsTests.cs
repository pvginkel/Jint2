using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Mozilla
{
    public class OperatorsTests : MozillaFixture
    {
        public OperatorsTests()
            : base("Operators")
        {
        }

        [TestCase("11.13.1-001.js")]
        [TestCase("11.13.1-002.js")]
        [TestCase("11.4.1-001.js")]
        [TestCase("11.4.1-002.js")]
        [TestCase("order-01.js")]
        public void ShouldRunOperatorsTests(string script)
        {
            RunFile(script);
        }
    }
}
