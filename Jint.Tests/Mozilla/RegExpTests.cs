using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Mozilla
{
    public class RegExpTests : MozillaFixture
    {
        public RegExpTests()
            : base("RegExp")
        {
        }

        [TestCase("15.10.2.12.js")]
        [TestCase("15.10.2-1.js")]
        [TestCase("15.10.3.1-1.js")]
        [TestCase("15.10.3.1-2.js")]
        [TestCase("15.10.4.1-1.js")]
        [TestCase("15.10.4.1-2.js")]
        [TestCase("15.10.4.1-3.js")]
        [TestCase("15.10.4.1-4.js")]
        [TestCase("15.10.4.1-5-n.js")]
        [TestCase("15.10.4.1-6.js")]
        [TestCase("15.10.6.2-1.js")]
        [TestCase("15.10.6.2-2.js")]
        [TestCase("octal-001.js")]
        [TestCase("octal-002.js")]
        [TestCase("perlstress-001.js")]
        [TestCase("perlstress-002.js")]
        [TestCase("regress-100199.js")]
        [TestCase("regress-105972.js")]
        [TestCase("regress-119909.js")]
        [TestCase("regress-122076.js")]
        [TestCase("regress-123437.js")]
        [TestCase("regress-165353.js")]
        [TestCase("regress-169497.js")]
        [TestCase("regress-169534.js")]
        [TestCase("regress-187133.js")]
        [TestCase("regress-188206.js")]
        [TestCase("regress-191479.js")]
        [TestCase("regress-202564.js")]
        [TestCase("regress-209067.js")]
        [TestCase("regress-209919.js")]
        [TestCase("regress-216591.js")]
        [TestCase("regress-220367-001.js")]
        [TestCase("regress-223273.js")]
        [TestCase("regress-223535.js")]
        [TestCase("regress-224676.js")]
        [TestCase("regress-225289.js")]
        [TestCase("regress-225343.js")]
        [TestCase("regress-24712.js")]
        [TestCase("regress-285219.js")]
        [TestCase("regress-28686.js")]
        [TestCase("regress-289669.js")]
        // [TestCase("regress-307456.js")] // TODO: This one hangs
        [TestCase("regress-309840.js")]
        [TestCase("regress-311414.js")]
        [TestCase("regress-312351.js")]
        [TestCase("regress-31316.js")]
        // [TestCase("regress-330684.js")] // TODO: This one hangs
        [TestCase("regress-334158.js")]
        [TestCase("regress-346090.js")]
        [TestCase("regress-367888.js")]
        [TestCase("regress-375642.js")]
        [TestCase("regress-375711.js")]
        [TestCase("regress-375715-01-n.js")]
        [TestCase("regress-375715-02.js")]
        [TestCase("regress-375715-03.js")]
        [TestCase("regress-375715-04.js")]
        [TestCase("regress-436700.js")]
        [TestCase("regress-465862.js")]
        [TestCase("regress-57572.js")]
        [TestCase("regress-57631.js")]
        [TestCase("regress-67773.js")]
        [TestCase("regress-72964.js")]
        [TestCase("regress-76683.js")]
        [TestCase("regress-78156.js")]
        [TestCase("regress-85721.js")]
        [TestCase("regress-87231.js")]
        [TestCase("regress-98306.js")]
        public void ShouldRunRegExpTests(string script)
        {
            RunFile(script);
        }
    }
}
