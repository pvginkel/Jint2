using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Mozilla
{
    public class UnicodeTests : MozillaFixture
    {
        public UnicodeTests()
            : base("Unicode")
        {
        }

        [TestCase("regress-352044-01.js")]
        [TestCase("regress-352044-02-n.js")]
        [TestCase("uc-001.js")]
        [TestCase("uc-001-n.js")]
        [TestCase("uc-002.js")]
        [TestCase("uc-002-n.js")]
        [TestCase("uc-003.js")]
        [TestCase("uc-004.js")]
        [TestCase("uc-005.js")]
        public void ShouldRunUnicodeTests(string script)
        {
            RunFile(script);
        }
    }
}
