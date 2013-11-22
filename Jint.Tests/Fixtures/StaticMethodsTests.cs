using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class StaticMethodsTests : FixturesFixture
    {
        public StaticMethodsTests()
            : base(null)
        {
        }

        [TestCase("StaticMethods.js")]
        public void ShouldRunStaticMethodsTests(string script)
        {
            RunFile(script);
        }
    }
}
