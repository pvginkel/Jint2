using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class TernaryTests : FixturesFixture
    {
        public TernaryTests()
            : base(null)
        {
        }

        [TestCase("Ternary.js")]
        public void ShouldRunTernaryTests(string script)
        {
            RunFile(script);
        }
    }
}
