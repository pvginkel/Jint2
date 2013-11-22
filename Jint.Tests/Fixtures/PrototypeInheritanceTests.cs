using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class PrototypeInheritanceTests : FixturesFixture
    {
        public PrototypeInheritanceTests()
            : base(null)
        {
        }

        [TestCase("PrototypeInheritance.js")]
        public void ShouldRunPrototypeInheritanceTests(string script)
        {
            RunFile(script);
        }
    }
}
