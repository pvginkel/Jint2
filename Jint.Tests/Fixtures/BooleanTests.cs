using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class BooleanTests : FixturesFixture
    {
        public BooleanTests()
            : base(null)
        {
        }

        [TestCase("Boolean.js")]
        public void ShouldRunBooleanTests(string script)
        {
            RunFile(script);
        }
    }
}
