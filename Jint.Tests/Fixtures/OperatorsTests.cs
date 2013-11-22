using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class OperatorsTests : FixturesFixture
    {
        public OperatorsTests()
            : base(null)
        {
        }

        [TestCase("Operators.js")]
        public void ShouldRunOperatorsTests(string script)
        {
            RunFile(script);
        }
    }
}
