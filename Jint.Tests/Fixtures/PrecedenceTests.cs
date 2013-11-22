using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class PrecedenceTests : FixturesFixture
    {
        public PrecedenceTests()
            : base(null)
        {
        }

        [TestCase("Precedence.js")]
        public void ShouldRunPrecedenceTests(string script)
        {
            RunFile(script);
        }
    }
}
