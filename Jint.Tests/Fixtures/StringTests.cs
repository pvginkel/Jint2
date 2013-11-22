using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class StringTests : FixturesFixture
    {
        public StringTests()
            : base(null)
        {
        }

        [TestCase("String.js")]
        public void ShouldRunStringTests(string script)
        {
            RunFile(script);
        }
    }
}
