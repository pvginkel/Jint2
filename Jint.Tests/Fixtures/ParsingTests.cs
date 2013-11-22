using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Fixtures
{
    [Ignore("These take very long to run and don't work at the moment")]
    public class ParsingTests : FixturesFixture
    {
        public ParsingTests()
            : base("Parsing")
        {
        }

        [TestCase("jquery.js")]
        [TestCase("jquery-1.2.6.js")]
        [TestCase("jquery-1.2.6.min.js")]
        [TestCase("jquery-1.3.2.min.js")]
        [TestCase("jquery-1.4.1.js")]
        [TestCase("jquery-1.4.1.min.js")]
        [TestCase("sizzle.js")]
        public void ShouldRunParsingTests(string script)
        {
            RunFile(script);
        }
    }
}
