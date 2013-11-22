using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Mozilla
{
    public class ExtensionsTests : MozillaFixture
    {
        public ExtensionsTests()
            : base("Extensions")
        {
        }

        [TestCase("10.1.3-2.js")]
        [TestCase("7.9.1.js")]
        // [TestCase("regress-103087.js")] TODO: This one hangs
        [TestCase("regress-188206-01.js")]
        [TestCase("regress-188206-02.js")]
        [TestCase("regress-220367-002.js")]
        [TestCase("regress-228087.js")]
        [TestCase("regress-274152.js")]
        [TestCase("regress-320854.js")]
        [TestCase("regress-327170.js")]
        [TestCase("regress-368516.js")]
        [TestCase("regress-385393-03.js")]
        [TestCase("regress-429248.js")]
        [TestCase("regress-430740.js")]
        public void ShouldRunExtensionsTests(string script)
        {
            RunFile(script);
        }
    }
}
