using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class WithTests : FixturesFixture
    {
        public WithTests()
            : base(null)
        {
        }

        [TestCase("With.js")]
        public void ShouldRunWithTests(string script)
        {
            RunFile(script);
        }
    }
}
