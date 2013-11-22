using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Mozilla
{
    public class StatementsTests : MozillaFixture
    {
        public StatementsTests()
            : base("Statements")
        {
        }

        [TestCase("12.10-01.js")]
        [TestCase("12.6.3.js")]
        [TestCase("regress-121744.js")]
        [TestCase("regress-131348.js")]
        [TestCase("regress-157509.js")]
        [TestCase("regress-194364.js")]
        [TestCase("regress-226517.js")]
        [TestCase("regress-302439.js")]
        [TestCase("regress-324650.js")]
        // [TestCase("regress-444979.js")] // TODO: This one hangs
        [TestCase("regress-74474-001.js")]
        [TestCase("regress-74474-002.js")]
        [TestCase("regress-74474-003.js")]
        [TestCase("regress-83532-001.js")]
        [TestCase("regress-83532-002.js")]
        [TestCase("switch-001.js")]
        public void ShouldRunStatementsTests(string script)
        {
            RunFile(script);
        }
    }
}
