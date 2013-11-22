using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class LoopsTests : FixturesFixture
    {
        public LoopsTests()
            : base(null)
        {
        }

        [TestCase("Loops.js")]
        public void ShouldRunLoopsTests(string script)
        {
            RunFile(script);
        }
    }
}
