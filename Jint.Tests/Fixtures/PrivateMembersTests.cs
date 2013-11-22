using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class PrivateMembersTests : FixturesFixture
    {
        public PrivateMembersTests()
            : base(null)
        {
        }

        [TestCase("PrivateMembers.js")]
        public void ShouldRunPrivateMembersTests(string script)
        {
            RunFile(script);
        }
    }
}
