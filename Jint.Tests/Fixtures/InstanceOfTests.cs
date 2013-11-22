using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class InstanceOfTests : FixturesFixture
    {
        public InstanceOfTests()
            : base(null)
        {
        }

        [TestCase("InstanceOf.js")]
        public void ShouldRunInstanceOfTests(string script)
        {
            RunFile(script);
        }
    }
}
