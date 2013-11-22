using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class ArgumentsTests : FixturesFixture
    {
        public ArgumentsTests()
            : base(null)
        {
        }

        [TestCase("Arguments.js")]
        public void ShouldRunArgumentsTests(string script)
        {
            RunFile(script);
        }
    }
}
