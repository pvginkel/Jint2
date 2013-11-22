using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    public class CommentsTests : FixturesFixture
    {
        public CommentsTests()
            : base(null)
        {
        }

        [TestCase("Comments.js")]
        public void ShouldRunCommentsTests(string script)
        {
            RunFile(script);
        }
    }
}
