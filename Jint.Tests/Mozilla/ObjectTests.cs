using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Mozilla
{
    public class ObjectTests : MozillaFixture
    {
        public ObjectTests()
            : base("Object")
        {
        }

        [TestCase("8.6.1-01.js")]
        [TestCase("8.6.2.6-001.js")]
        [TestCase("8.6.2.6-002.js")]
        [TestCase("class-001.js")]
        [TestCase("class-002.js")]
        [TestCase("class-003.js")]
        [TestCase("class-004.js")]
        [TestCase("class-005.js")]
        [TestCase("regress-361274.js")]
        [TestCase("regress-385393-07.js")]
        [TestCase("regress-459405.js")]
        [TestCase("regress-72773.js")]
        [TestCase("regress-79129-001.js")]
        public void ShouldRunObjectTests(string script)
        {
            RunFile(script);
        }
    }
}
