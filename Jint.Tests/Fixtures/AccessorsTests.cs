using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class AccessorsTests : FixturesFixture
    {
        public AccessorsTests()
            : base(null)
        {
        }

        [TestCase("Accessors.js")]
        public void ShouldRunAccessorsTests(string script)
        {
            RunFile(script);
        }
    }
}
