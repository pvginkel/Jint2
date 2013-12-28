using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class StrictTests : FixturesFixture
    {
        public StrictTests()
            : base(null)
        {
        }

        [TestCase("Strict.js")]
        public void ShouldRunStrictTests(string script)
        {
            RunFile(script);
        }
    }
}
