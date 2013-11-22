using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class TryCatchTests : FixturesFixture
    {
        public TryCatchTests()
            : base(null)
        {
        }

        [TestCase("TryCatch.js")]
        public void ShouldRunTryCatchTests(string script)
        {
            RunFile(script);
        }
    }
}
