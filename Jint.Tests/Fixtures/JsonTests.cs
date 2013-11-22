using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class JsonTests : FixturesFixture
    {
        public JsonTests()
            : base(null)
        {
        }

        [TestCase("Json.js")]
        public void ShouldRunJsonTests(string script)
        {
            RunFile(script);
        }
    }
}
