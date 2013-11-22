using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class NumberTests : FixturesFixture
    {
        public NumberTests()
            : base(null)
        {
        }

        [TestCase("Number.js")]
        public void ShouldRunNumberTests(string script)
        {
            RunFile(script);
        }
    }
}
