using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class HoistingTests : FixturesFixture
    {
        public HoistingTests()
            : base(null)
        {
        }

        [TestCase("Hoisting.js")]
        public void ShouldRunHoistingTests(string script)
        {
            RunFile(script);
        }
    }
}
