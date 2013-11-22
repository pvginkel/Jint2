using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class RegExpTests : FixturesFixture
    {
        public RegExpTests()
            : base(null)
        {
        }

        [TestCase("RegExp.js")]
        public void ShouldRunRegExpTests(string script)
        {
            RunFile(script);
        }
    }
}
