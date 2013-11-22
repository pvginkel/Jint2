using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class SimpleClassTests : FixturesFixture
    {
        public SimpleClassTests()
            : base(null)
        {
        }

        [TestCase("SimpleClass.js")]
        public void ShouldRunSimpleClassTests(string script)
        {
            RunFile(script);
        }
    }
}
