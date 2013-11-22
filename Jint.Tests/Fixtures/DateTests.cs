using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class DateTests : FixturesFixture
    {
        public DateTests()
            : base(null)
        {
        }

        [TestCase("Date.js")]
        public void ShouldRunDateTests(string script)
        {
            RunFile(script);
        }
    }
}
