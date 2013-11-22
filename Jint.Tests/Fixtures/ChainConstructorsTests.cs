using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class ChainConstructorsTests : FixturesFixture
    {
        public ChainConstructorsTests()
            : base(null)
        {
        }

        [TestCase("ChainConstructors.js")]
        public void ShouldRunChainConstructorsTests(string script)
        {
            RunFile(script);
        }
    }
}
