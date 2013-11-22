using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class ClosuresTests : FixturesFixture
    {
        public ClosuresTests()
            : base(null)
        {
        }

        [TestCase("Closures.js")]
        public void ShouldRunClosuresTests(string script)
        {
            RunFile(script);
        }
    }
}
