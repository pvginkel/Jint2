using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class ObjectTests : FixturesFixture
    {
        public ObjectTests()
            : base(null)
        {
        }

        [TestCase("Object.js")]
        public void ShouldRunObjectTests(string script)
        {
            RunFile(script);
        }
    }
}
