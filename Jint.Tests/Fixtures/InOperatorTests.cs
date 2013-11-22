using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class InOperatorTests : FixturesFixture
    {
        public InOperatorTests()
            : base(null)
        {
        }

        [TestCase("InOperator.js")]
        public void ShouldRunInOperatorTests(string script)
        {
            RunFile(script);
        }
    }
}
