using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class BlocksTests : FixturesFixture
    {
        public BlocksTests()
            : base(null)
        {
        }

        [TestCase("Blocks.js")]
        public void ShouldRunBlocksTests(string script)
        {
            RunFile(script);
        }
    }
}
