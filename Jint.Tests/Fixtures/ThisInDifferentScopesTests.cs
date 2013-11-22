using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class ThisInDifferentScopesTests : FixturesFixture
    {
        public ThisInDifferentScopesTests()
            : base(null)
        {
        }

        [TestCase("ThisInDifferentScopes.js")]
        public void ShouldRunThisInDifferentScopesTests(string script)
        {
            RunFile(script);
        }
    }
}
