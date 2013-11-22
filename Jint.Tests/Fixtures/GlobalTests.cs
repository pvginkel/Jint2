using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class GlobalTests : FixturesFixture
    {
        public GlobalTests()
            : base(null)
        {
        }

        [TestCase("Global.js")]
        public void ShouldRunGlobalTests(string script)
        {
            RunFile(script);
        }
    }
}
